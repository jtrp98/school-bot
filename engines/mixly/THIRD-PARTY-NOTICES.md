# Mixly engine third-party notices

The School Bot adapter is separate from upstream Mixly. Mixly's name, authorship,
source headers and license are retained. `source-manifest.json` records the exact
imported files using SHA256 over UTF-8 text with LF endings. The original package
identifies its author as Mixly and version as `2.0rc3`.

| Component | Pinned version/source | License text shipped in `licenses/` |
| --- | --- | --- |
| Mixly generators, board profiles and configuration | `sb-app-mixly/resources/app/src`; per-file hashes in `source-manifest.json` | `MIXLY-LICENSE.txt` (original retention terms) |
| Blockly bundled by Mixly; Google Inc. and contributors | The imported Mixly snapshot; original copyright/author headers retained | `BLOCKLY-LICENSE.txt` (Apache-2.0) |
| Go standard library | Go toolchain used by the build; minimum Go 1.26 | `GO-LICENSE.txt` (BSD-3-Clause) |
| github.com/dop251/goja | v0.0.0-20261020172427-7b1a02208696 | `GOJA-LICENSE.txt` (MIT) |
| github.com/dlclark/regexp2/v2 | v2.8.1 | `REGEXP2-LICENSE.txt` (MIT) |
| github.com/go-sourcemap/sourcemap | v2.1.3+incompatible | `SOURCEMAP-LICENSE.txt` (BSD-2-Clause) |
| github.com/google/pprof | v0.0.0-20230207041349-798e818bf904 | `PPROF-LICENSE.txt` (Apache-2.0) |
| go.bug.st/serial | v1.6.4 | `SERIAL-LICENSE.txt` (BSD-3-Clause) |
| github.com/creack/goselect | v0.1.2 | `GOSELECT-LICENSE.txt` (MIT) |
| golang.org/x/sys | v0.19.0 | `X-SYS-LICENSE.txt` (BSD-3-Clause) |
| golang.org/x/text | v0.43.0 | `X-TEXT-LICENSE.txt` (BSD-3-Clause) |

Upstream repository: https://gitee.com/mixly2/mixly2.0_src

The upstream AVR serial sample in `tests/fixtures/avr-serial.mix` is test input
from `sample/Arduino AVR/04-文本/01-serial_string-1.mix`; it is not in the binary.

The engine does not redistribute Electron, Arduino CLI, Python, mpremote, esptool,
kflash, board cores, firmware images or external libraries. A toolchain distribution
must preserve those components' own notices and licenses when it supplies them.
The engine's binary embeds the imported generator text. This is not encryption
and does not prevent inspection or reverse engineering.
