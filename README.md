# Collection of Datastructures and Algorithms

Implementing some common datastructures in C-Sharp. (and copying in some `go`, `rust` or `C` examples for comparison if i alredy wrote those previously)
- Goal is mostly to get familiar with Csharp.

## Implemented are roughly


- [x] Dynamic Array
- [x] Stack
- [x] Linked List (singly/double)
- [x] Queue (linkedlist/array)
- [x] Priority-Queue/Heap (min/max)
- [x] Union Find (kruskals Algorithm)
- [x] Binary Search Tree
    - insert
    - remove
    - preorder, inorder, postorder, level-order, traversals
    - 
- [] BBST Balanced Binary Search Tree
    - AVL tree insertion
    - AVL tree removal
https://www.youtube.com/watch?v=RBSGKlAvoiM&t=9631s
- [x] Hash Table
    - [x] separate chaining
    - [x] open adressing
    - [x] linear probing
    - quadratic probing
    - double hashing
    - open adressing removing
- [] Fenwick Tree range queries
    - range queries, point updates
- [] Suffix Array
    - longest common prefix array
    - finding unique substrings
    - longest common substring problem
- [] indexed Priority Queue
- [] Arena allocator (should probably in c/rust unsafe)
https://www.youtube.com/watch?v=09_LlHjoEiY&list=PLWKjhJtqVAbn5emQ3RRG8gEBqkhf_5vxD&index=14

- graph theory, sorting/path algorithms etc.

# Big-O Notation
From good to bad:

|time| notation|
|---|---|
|Constant Time| O(1) |
|Logarithmic Time| O(lon(n)) |
|Linear Time| O(n) |
|Linearithmic Time| O(nlog(n)) |
|Quadratic Time| O(n²) |
|Cubic Time| O(n³) |
|Exponential Time| O(b^n)|
|Factorial Time| O(n!) |

## Properties
Big O notation only cares about the limit, when n gets really big. So 
- constants get ignored `9999 + n³ -> O(n³)`
- factors get ignored `9999*n² -> O(n²)`
- only the fastest growing factor becomes the O-Value `log(n)⁴ + 2n⁴ + 88n² -> O(n⁴)`

## Examples
### Constant time
```cs
c = a + 5*b / 12;
// since the loop always runs the same ammount of times -> constant (not coupled with n)
for (int i=0; i<99999; i++) {
    //do things here
}
```

### Linear time
```cs
var i = 0;
while (i<n) {
    i = i+1;
}

i = 0;
while (i<n) {
    i + 1000;
}
```
both blocks are `O(n)`

### Quadratic time
```cs
for (int i=0; i<n; i++) {
    for (int j=0; j<n; j++){
        // ...
    }
}
// fn(n) = n*n = n² -> O(fn(n)) = O(n²)

for (int i=0; i<n; i++) {
    for (int j=i; j<n; j++) {    //replaced  =0 with =i
        // ...
    }
}
// fn(n) = n*n = n² -> O(fn(n)) = O(n²)
```