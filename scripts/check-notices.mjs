import { execFileSync } from 'node:child_process';
import { createHash } from 'node:crypto';
import { existsSync, readFileSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const root = path.resolve(fileURLToPath(new URL('..', import.meta.url)));
const read = (relativePath) => readFileSync(path.join(root, relativePath), 'utf8');
const problems = [];

const notices = read('THIRD-PARTY-NOTICES.md');
const noticeLines = notices.split('\n');

function listed(name, version) {
  return noticeLines.some((line) => line.includes(`\`${name}\``) && line.includes(version));
}

function portableGo() {
  const executable = process.platform === 'win32' ? 'go.exe' : 'go';
  const portable = path.join(root, '.tools', 'go', 'bin', executable);
  if (!existsSync(portable)) return { command: 'go', env: process.env };
  const env = {
    ...process.env,
    GOPATH: path.join(root, '.tools', 'gopath'),
    GOCACHE: path.join(root, '.tools', 'gocache'),
  };
  return { command: portable, env };
}

// Windows and macOS link different module sets, so each shipped target is listed on its own.
const goTargets = [
  { goos: 'windows', goarch: 'amd64', cgo: '0' },
  { goos: 'darwin', goarch: 'arm64', cgo: '1' },
  { goos: 'darwin', goarch: 'amd64', cgo: '1' },
];

function goModules(target, go) {
  const template = '{{if not .Standard}}{{with .Module}}{{if not .Main}}{{.Path}} {{.Version}}{{end}}{{end}}{{end}}';
  const output = execFileSync(
    go.command,
    ['list', '-mod=readonly', '-deps', '-tags', 'production', '-f', template, './...'],
    {
      cwd: path.join(root, 'app'),
      encoding: 'utf8',
      env: { ...go.env, GOOS: target.goos, GOARCH: target.goarch, CGO_ENABLED: target.cgo },
    },
  );
  return output.split('\n').map((line) => line.trim()).filter(Boolean);
}

function checkGoModules() {
  const go = portableGo();
  const modules = new Set();
  for (const target of goTargets) {
    for (const entry of goModules(target, go)) modules.add(entry);
  }
  for (const entry of [...modules].sort()) {
    const [name, version] = entry.split(' ');
    if (listed(name, version)) continue;
    problems.push(`Go module ${name} ${version} is linked but not in THIRD-PARTY-NOTICES.md`);
  }
  if (!noticeLines.some((line) => line.includes('Go standard library'))) {
    problems.push('Go standard library is linked but not in THIRD-PARTY-NOTICES.md');
  }
  return modules.size;
}

// devDependencies are build tools and are not part of dist, so only runtime dependencies are followed.
function runtimeNpmPackages() {
  const lock = JSON.parse(read('app/frontend/package-lock.json'));
  const packages = lock.packages;
  const visited = new Map();
  const locate = (from, name) => {
    let scope = from;
    for (;;) {
      const key = `${scope ? `${scope}/` : ''}node_modules/${name}`;
      if (packages[key]) return key;
      if (!scope) return null;
      const cut = scope.lastIndexOf('/node_modules/');
      scope = cut < 0 ? '' : scope.slice(0, cut);
    }
  };
  const visit = (key) => {
    if (visited.has(key)) return;
    const entry = packages[key];
    const name = key.slice(key.lastIndexOf('node_modules/') + 'node_modules/'.length);
    visited.set(key, { name, version: entry.version });
    const runtime = { ...entry.dependencies, ...entry.optionalDependencies };
    for (const dependency of Object.keys(runtime)) {
      const child = locate(key, dependency);
      if (child) visit(child);
    }
  };
  for (const dependency of Object.keys(packages[''].dependencies)) {
    const key = locate('', dependency);
    if (!key) problems.push(`package-lock.json has no entry for dependency ${dependency}`);
    else visit(key);
  }
  return [...visited.values()];
}

function checkNpmPackages() {
  const packages = runtimeNpmPackages();
  for (const { name, version } of packages) {
    if (listed(name, version)) continue;
    problems.push(`npm package ${name} ${version} is bundled but not in THIRD-PARTY-NOTICES.md`);
  }
  return packages.length;
}

// Hashes use LF endings because git checks the vendored text out with platform line endings.
const vendoredFiles = [
  'engines/zide/third_party/hidapi/windows/hid.c',
  'engines/zide/third_party/hidapi/hidapi/hidapi.h',
  'engines/zide/third_party/nlohmann/json.hpp',
];

function checkEngineNotices() {
  const engineNotices = read('engines/zide/THIRD-PARTY-NOTICES.md');
  for (const file of vendoredFiles) {
    const text = read(file).replace(/\r\n/g, '\n');
    const hash = createHash('sha256').update(text).digest('hex');
    if (!engineNotices.includes(hash)) {
      problems.push(`${file} changed (sha256 ${hash}) but engines/zide/THIRD-PARTY-NOTICES.md records another hash`);
    }
  }
  for (const license of ['third_party/hidapi/LICENSE-bsd.txt', 'third_party/nlohmann/LICENSE.MIT']) {
    if (!existsSync(path.join(root, 'engines/zide', license))) problems.push(`engines/zide/${license} is missing`);
  }
  for (const name of ['HIDAPI', 'JSON for Modern C++']) {
    if (notices.includes(name)) continue;
    problems.push(`${name} is compiled into the Zide engine but not in THIRD-PARTY-NOTICES.md`);
  }
}

const goCount = checkGoModules();
const npmCount = checkNpmPackages();
checkEngineNotices();

if (problems.length > 0) {
  console.error(`Third-party notices are incomplete:\n${problems.map((problem) => `- ${problem}`).join('\n')}`);
  process.exit(1);
}
console.log(`Third-party notices cover ${goCount} Go modules, ${npmCount} npm packages and the Zide vendored files.`);
