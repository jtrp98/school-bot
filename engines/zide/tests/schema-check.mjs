import { readFileSync } from 'node:fs';

// Covers only the JSON Schema keywords the engine contracts use, so the test needs no new dependency.
// An unknown keyword throws instead of being skipped, otherwise a schema change could pass unchecked.
const ANNOTATIONS = new Set(['$schema', '$id', 'title', 'description', '$defs']);
const types = {
  object: value => value !== null && typeof value === 'object' && !Array.isArray(value),
  string: value => typeof value === 'string',
};

export const loadSchema = url => JSON.parse(readFileSync(url, 'utf8'));

function resolve(root, ref) {
  if (!ref.startsWith('#/')) throw new Error(`unsupported $ref ${ref}`);
  return ref.slice(2).split('/').reduce((node, key) => node[key], root);
}

export function violations(schema, value, root = schema, path = '$') {
  const found = [];
  for (const keyword of Object.keys(schema)) {
    if (ANNOTATIONS.has(keyword)) continue;
    switch (keyword) {
      case '$ref':
        found.push(...violations(resolve(root, schema.$ref), value, root, path));
        break;
      case 'const':
        if (value !== schema.const) found.push(`${path}: expected ${JSON.stringify(schema.const)}`);
        break;
      case 'enum':
        if (!schema.enum.includes(value)) found.push(`${path}: ${JSON.stringify(value)} is not an allowed value`);
        break;
      case 'type':
        if (!types[schema.type](value)) found.push(`${path}: expected ${schema.type}`);
        break;
      case 'minLength':
        if (typeof value === 'string' && [...value].length < schema.minLength) found.push(`${path}: too short`);
        break;
      case 'maxLength':
        if (typeof value === 'string' && [...value].length > schema.maxLength) found.push(`${path}: too long`);
        break;
      case 'required':
        if (types.object(value)) {
          for (const key of schema.required) if (!(key in value)) found.push(`${path}: missing ${key}`);
        }
        break;
      case 'properties':
        if (types.object(value)) {
          for (const [key, child] of Object.entries(schema.properties)) {
            if (key in value) found.push(...violations(child, value[key], root, `${path}.${key}`));
          }
        }
        break;
      case 'additionalProperties':
        if (schema.additionalProperties !== false) throw new Error('only additionalProperties:false is supported');
        if (types.object(value)) {
          for (const key of Object.keys(value)) {
            if (!(key in (schema.properties ?? {}))) found.push(`${path}: unexpected ${key}`);
          }
        }
        break;
      case 'not':
        if (violations(schema.not, value, root, path).length === 0) found.push(`${path}: matches a forbidden schema`);
        break;
      case 'allOf':
        for (const part of schema.allOf) found.push(...violations(part, value, root, path));
        break;
      case 'if': {
        const branch = violations(schema.if, value, root, path).length === 0 ? schema.then : schema.else;
        if (branch) found.push(...violations(branch, value, root, path));
        break;
      }
      case 'then':
      case 'else':
        break;
      default:
        throw new Error(`unsupported schema keyword ${keyword}`);
    }
  }
  return found;
}
