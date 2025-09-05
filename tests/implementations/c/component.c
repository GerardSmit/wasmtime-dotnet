#include "test.h"
#include <stdlib.h>
#include <string.h>
#include <ctype.h>
#include <stdio.h>

// Global storage for entities
static test_entity_t *g_entities = NULL;
static size_t g_entities_count = 0;
static size_t g_entities_capacity = 0;

// Global flag storage
static bool g_flag = false;

// Helper function to grow entity storage
static void ensure_entity_capacity(size_t needed) {
    if (g_entities_capacity >= needed) return;
    
    size_t new_capacity = g_entities_capacity == 0 ? 8 : g_entities_capacity * 2;
    while (new_capacity < needed) new_capacity *= 2;
    
    g_entities = realloc(g_entities, new_capacity * sizeof(test_entity_t));
    g_entities_capacity = new_capacity;
}

// Helper function to duplicate a string
static test_string_t duplicate_string(const test_string_t *src) {
    test_string_t result;
    result.len = src->len;
    result.ptr = malloc(src->len);
    memcpy(result.ptr, src->ptr, src->len);
    return result;
}

// Helper function to duplicate an entity
static test_entity_t duplicate_entity(const test_entity_t *src) {
    test_entity_t result;
    result.id = src->id;
    result.name = duplicate_string(&src->name);
    result.position = src->position;
    return result;
}

// Arithmetic functions
uint8_t exports_test_add_u8(uint8_t x, uint8_t y) {
    return x + y;
}

int8_t exports_test_add_s8(int8_t x, int8_t y) {
    return x + y;
}

uint16_t exports_test_add_u16(uint16_t x, uint16_t y) {
    return x + y;
}

int16_t exports_test_add_s16(int16_t x, int16_t y) {
    return x + y;
}

uint32_t exports_test_add_u32(uint32_t x, uint32_t y) {
    return x + y;
}

int32_t exports_test_add_s32(int32_t x, int32_t y) {
    return x + y;
}

uint64_t exports_test_add_u64(uint64_t x, uint64_t y) {
    return x + y;
}

int64_t exports_test_add_s64(int64_t x, int64_t y) {
    return x + y;
}

float exports_test_add_f32(float x, float y) {
    return x + y;
}

double exports_test_add_f64(double x, double y) {
    return x + y;
}

void exports_test_add_point(test_point_t *p1, test_point_t *p2, test_point_t *ret) {
    ret->x = p1->x + p2->x;
    ret->y = p1->y + p2->y;
}

// String functions
void exports_test_uppercase(test_string_t *s, test_string_t *ret) {
    ret->len = s->len;
    ret->ptr = malloc(s->len);
    
    for (size_t i = 0; i < s->len; i++) {
        ret->ptr[i] = toupper(s->ptr[i]);
    }
}

void exports_test_combine_string(test_string_t *s1, test_string_t *s2, test_string_t *ret) {
    ret->len = s1->len + s2->len;
    ret->ptr = malloc(ret->len);
    
    memcpy(ret->ptr, s1->ptr, s1->len);
    memcpy(ret->ptr + s1->len, s2->ptr, s2->len);
}

void exports_test_accept_string(test_string_t *s) {
    test_string_free(s);
}

void exports_test_return_string(uint32_t length, test_string_t *ret) {
    ret->len = length;
    ret->ptr = malloc(length);
    memset(ret->ptr, 'a', length);
}

// List functions
void exports_test_multiply_list(test_list_s32_t *list, int32_t factor, test_list_s32_t *ret) {
    ret->len = list->len;
    ret->ptr = malloc(list->len * sizeof(int32_t));
    
    for (size_t i = 0; i < list->len; i++) {
        ret->ptr[i] = list->ptr[i] * factor;
    }
}

uint32_t exports_test_sum_nested_list(test_list_list_u32_t *nested) {
    uint32_t sum = 0;
    for (size_t i = 0; i < nested->len; i++) {
        for (size_t j = 0; j < nested->ptr[i].len; j++) {
            sum += nested->ptr[i].ptr[j];
        }
    }
    return sum;
}

// Status and Permission functions
test_status_t exports_test_return_status(test_status_t status) {
    return status;
}

void exports_test_return_status_list(test_list_status_t *statuses, test_list_status_t *ret) {
    ret->len = statuses->len;
    ret->ptr = malloc(statuses->len * sizeof(test_status_t));
    memcpy(ret->ptr, statuses->ptr, statuses->len * sizeof(test_status_t));
}

test_permission_t exports_test_return_permission(test_permission_t permission) {
    return permission;
}

void exports_test_return_permission_list(test_list_permission_t *permissions, test_list_permission_t *ret) {
    ret->len = permissions->len;
    ret->ptr = malloc(permissions->len * sizeof(test_permission_t));
    memcpy(ret->ptr, permissions->ptr, permissions->len * sizeof(test_permission_t));
}

// Entity management functions
void exports_test_register_entity(test_entity_t *e) {
    // Check if entity with this ID already exists
    for (size_t i = 0; i < g_entities_count; i++) {
        if (g_entities[i].id == e->id) {
            // Update existing entity
            test_string_free(&g_entities[i].name);
            g_entities[i] = duplicate_entity(e);
            return;
        }
    }
    
    // Add new entity
    ensure_entity_capacity(g_entities_count + 1);
    g_entities[g_entities_count] = duplicate_entity(e);
    g_entities_count++;
}

void exports_test_register_entities(test_list_entity_t *e) {
    for (size_t i = 0; i < e->len; i++) {
        exports_test_register_entity(&e->ptr[i]);
    }
}

void exports_test_get_entity(int32_t id, test_entity_t *ret) {
    for (size_t i = 0; i < g_entities_count; i++) {
        if (g_entities[i].id == id) {
            *ret = duplicate_entity(&g_entities[i]);
            return;
        }
    }
    
    // Return default entity if not found
    ret->id = -1;
    test_string_dup(&ret->name, "");
    ret->position.x = 0;
    ret->position.y = 0;
}

void exports_test_get_entities(test_list_entity_t *ret) {
    ret->len = g_entities_count;
    ret->ptr = malloc(g_entities_count * sizeof(test_entity_t));
    
    for (size_t i = 0; i < g_entities_count; i++) {
        ret->ptr[i] = duplicate_entity(&g_entities[i]);
    }
}

// Flag functions
void exports_test_set_flag(bool flag) {
    g_flag = flag;
}

bool exports_test_get_flag(void) {
    return g_flag;
}

// Host interaction functions
void exports_test_get_host_entity_description(test_string_t *ret) {
    test_entity_t entity;
    test_get_host_entity(&entity);
    
    // Create description string
    char description[256];
    size_t name_len = entity.name.len;
    char *name_str = malloc(name_len + 1);
    memcpy(name_str, entity.name.ptr, name_len);
    name_str[name_len] = '\0';
    
    int desc_len = snprintf(description, sizeof(description), "Entity %d: %s", entity.id, name_str);
    
    test_string_dup(ret, description);
    
    free(name_str);
    test_entity_free(&entity);
}

void exports_test_host_callback(void) {
    test_callback();
}

void exports_test_host_combine_string(test_string_t *s1, test_string_t *s2, test_string_t *ret) {
    test_callback_combine_string(s1, s2, ret);
}
