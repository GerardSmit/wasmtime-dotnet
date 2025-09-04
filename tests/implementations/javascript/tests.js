import callback from 'callback';
import callbackCombineString from 'callback-combine-string';
import getHostEntity from 'get-host-entity';

export function addU8(x, y) {
  return x + y;
}

export function addS8(x, y) {
  return x + y;
}

export function addU16(x, y) {
  return x + y;
}

export function addS16(x, y) {
  return x + y;
}

export function addU32(x, y) {
  return x + y;
}

export function addS32(x, y) {
  return x + y;
}

export function addU64(x, y) {
  return x + y;
}

export function addS64(x, y) {
  return x + y;
}

export function addF32(x, y) {
  return x + y;
}

export function addF64(x, y) {
  return x + y;
}

export function addPoint(p1, p2) {
  return {
    x: p1.x + p2.x,
    y: p1.y + p2.y
  };
}

export function uppercase(s) {
  return s.toUpperCase();
}

export function multiplyList(list, factor) {
  const results = [];
  
  for (const item of list) {
     results.push(item * factor);
  }

  return results;
}

export function returnStatus(status) {
  return status;
}

export function returnStatusList(statuses) {
  return statuses;
}

export function returnPermission(permission) {
  return permission;
}

export function returnPermissionList(permissions) {
  return permissions;
}

// Dummy entity storage
let entities = new Map();
export function registerEntity(entity) {
  entities.set(entity.id, entity);
}

export function registerEntities(entityList) {
  entityList.forEach(entity => {
    entities.set(entity.id, entity);
  });
}

export function getEntity(id) {
  return entities.get(id) || { id: -1, name: "", position: { x: 0, y: 0 } };
}

export function getEntities() {
  return Array.from(entities.values());
}

// Boolean flag storage
let globalFlag = false;

export function setFlag(flag) {
  globalFlag = flag;
}

export function getFlag() {
  return globalFlag;
}

export function getHostEntityDescription() {
  const entity = getHostEntity();
  return `Entity ${entity.id}: ${entity.name}`;
}

export function sumNestedList(nested) {
  let sum = 0;
  for (const list of nested) {
    for (const value of list) {
      sum += value;
    }
  }
  return sum;
}

export function hostCallback() {
  callback();
}

export function hostCombineString(s1, s2) {
  return callbackCombineString(s1, s2);
}

export function combineString(s1, s2) {
  return s1 + s2;
}

// Dummy implementations for memory
export function getMemoryUsage() {
  return 0.0;
}

export function forceGc() {
  
}

export function getMemoryTest() {
  return {
    gcHandles: 0,
    openGcHandles: 0,
    allocations: 0,
    openAllocations: 0
  };
}
