# Basic structures
The basic struct most other structs are made out of
## Static Array vs Dynamic Array
### Static Array
Continous chunk of memory. To reserve/allocate the memory the size has to be known at time of creation.
- This means they can be allocated on the stack (if whatever we store is fixed size). So really fast
- Modern CPU's are also really good at Cashing, so consecutive access to the same array can be really fast that way.
### Dynamic Array
Extension of a Statc Array but, gets reallocated once full (or if it shrinks enough).

|||||
|---|---|---|---|
|Access | O(1) | O(1) |can index directly into|
|Search | O(n) | O(n) |worst case whole array gets run trough|
|Insertion| N/A | O(n) |worst case we insert at idx=0 so we have to shift everything right|
|Appending| N/A | O(1) |really fast to `push()` or `pop()`|
|Deletion | N/A | O(n) |worst case we delete idx=0 so we have to shift everything left|

### Example Dynamic Array in C
First we take care of memory allocation with some generic macros:
- `memory.h`
```c
#ifndef custom_memory_h
#define custom_memory_h

// macro that allocates an array with given type and count/size:
#define ALLOCATE(type, count) \
    (type*)reallocate(NULL, 0, sizeof(type) * (count))

// macro - used to free memory that a custom Object used
#define FREE(type, pointer) reallocate(pointer, sizeof(type), 0)

// We define we double Capacity whenever we reach its limit
// - we start at 8 (after upgrade from when it gets initialized with capacity = 0)
// - afterwards we double each time 8 -> 16 -> 32 -> 64 -> 128....
#define GROW_CAPACITY(capacity) \
    ((capacity) < 8 ? 8 : (capacity) * 2)

// This macro calls reallocate with newSize=0 -> so it will get deleted from memory there
#define FREE_ARRAY(type, pointer, oldCount) \
    reallocate(pointer, sizeof(type) * (oldCount), 0);

void* reallocate(void* pointer, size_t oldSize, size_t newSize);
#endif
```
- `memory.c`
```c
#include <stdlib.h>
#include "memory.h"

//  The single function used for all dynamic memory management
    // if   -oldSize-   -newSize-       -then do Operation:-
    //      0           Non-zero        Allocate new block-
    //      Non-Zero    0               Free allocation.
    //      Non-Zero    new<oldSize     Shrink existing allocation.
    //      Non-Zero    new>oldSize     Grow existing allocation.
void* reallocate(void* pointer, size_t oldSize, size_t newSize) {
    if (newSize == 0) {
        free(pointer);
        return NULL;
    }

    void* result = realloc(pointer, newSize);
    if (result == NULL) exit(1);                // We must handle the case of realloc failing (ex. not enough free memory left on OS)
    return result;
}
```
- `array.h`
```c
#ifndef custom_array_h
#define custom_array_h

typedef struct {
    int capacity;
    int count;
    Int* values;    // pointer to our static-array
} IntArray;

void initIntArray(IntArray* array);
void pushToArray(IntArray* array, int value);
void deleteFromArray(IntArray* array, int index);
void freeIntArray(IntArray* array);


#endif
```
- `array.c`
```c
#include <stdio.h>
#include "memory.h"
#include "varray.h"

// inititializing the dynamic data structure
void initIntArray(IntArray* array) {
    array->values = NULL;
    array->capacity = 0;
    array->count = 0;
}

// appends new value to array, if full it reallocates to double'd size
void pushToArray(IntArray* array, int value) {
    if (array->capacity < array->count + 1) {
        int oldCapacity = array->capacity;
        array->capacity = GROW_CAPACITY(oldCapacity);
        array->values = GROW_ARRAY(Value, array->values, oldCapacity, array->capacity);
    }
    array->values[array->count] = value;
    array->count++;
}

// deletes at idx specified
void deleteFromArray(IntArray* array, int index) {
    for (int i = index; i < array->count -1; i++) {
        array->items[i] = array->items[i+1];
    }
    array->items[array->count-1] = NULL;    // cleanup the memory were not using anymore
    array->count--;
}

// after done we have to manually malloc the memory
void freeIntArray(IntArray* array) {
    FREE_ARRAY(Int, array->values, array->capacity);
    initIntArray(array);
}

void arrayWriteToIdx(IntArray* array, int index, int value) {
    array->items[index] = value;
}

int arrayReadFromIdx() {
    return array->values[index]
}

bool isValidIndex(IntArray* array, int index) {
    return (index >= 0 && index < array->count)
}

int arrayGetLength(IntArray* array) {
    return array->count;
}
```