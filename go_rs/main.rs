use std::{mem, ptr::NonNull, alloc::Layout};

fn main() {
    let mut arr:Array<&str> = Array::new();
    arr.push("Hello");
    arr.push("World");
    arr.push("1");
    arr.push("16");
    dbg!(&arr[0]);
    dbg!(&arr[1]);
    dbg!(&arr[3]);


}

#[derive(Debug)]
/// dynamic Array implementation - of note: Vec is already mostly what were building, so this is more a theoretical practice thing
pub struct Array<T> {
    buf: RawArray<T>,
    len: usize,
}

impl<T> Array<T> {
    /// just forwards our pointer from our RawArray
    fn ptr(&self) -> *mut T {
        self.buf.ptr.as_ptr()
    }

    /// just forwards our capacity from our RawArray
    fn cap(&self) -> usize {
        self.buf.cap
    }
    
    /// initializes the Array struct. BUT first allocation on the heap has not happened. (only when filled)
    pub fn new() -> Self {
        Array {
           buf: RawArray::new(),
            len: 0,
        }
    }

    /// pushes the element on top of the Array.
    pub fn push(&mut self, elem: T) {
        if self.len == self.cap() {self.buf.grow(); }
        unsafe {
            std::ptr::write(self.ptr().add(self.len), elem);
        }
        self.len += 1;
    }

    /// pops top element from Array, if it exists.
    pub fn pop(&mut self) -> Option<T> {
        if self.len == 0 {
            None
        } else {
            self.len -=1;
            unsafe {
                // ptr::read() copies the bits from adress and interprets it as T. 
                // we offset by self.len to get to the top of the stack
                Some(std::ptr::read(self.ptr().add(self.len)))
            }
        }
    }

    /// insert and shift to right if not inserting at top
    /// - we use ptr::copy == C's memmove -> copies a chunk of memory 
    pub fn insert(&mut self, index: usize, elem: T) {
        assert!(index <= self.len, "index is out of bounds.");
        if self.cap() == self.len { self.buf.grow(); }
        unsafe {
            // first we copy everything at the idx one idx to the right
            std::ptr::copy(
                self.ptr().add(index),
                self.ptr().add(index - 1),
                self.len - index,
            );
            // then we insert our new element at the index
            std::ptr::write(self.ptr().add(index), elem);
            self.len += 1;
        }
    }

    /// removes one element and shifts to left if there was something above it
    /// - we use ptr::copy == C's memmove -> copies a chunk of memory 
    pub fn remove(&mut self, index: usize) -> T {
        assert!(index < self.len, "index is out of bounds.");
        unsafe {
            self.len -=1;
            let result = std::ptr::read(
                self.ptr().add(index));
            std::ptr::copy(
                self.ptr().add(index +1),
                self.ptr().add(index),
                self.len - index,
            );
            return result;
        }
    }
}

impl <T> Drop for Array<T> {
    /// Drop trait - destructor that cleans up . we just do pop() till empty
    /// - Drop implementation for RawArray handles the Deallocation
    fn drop(&mut self) {
        while let Some(_) = self.pop() {}   // loop till empty
    }
}

// Deref to enable []- Syntax to rea values in the array. ex: 'dbg!(arr[1]);'
impl<T> std::ops::Deref for Array<T> {
    type Target = [T];
    fn deref(&self) -> &[T] {
        unsafe {
            std::slice::from_raw_parts(self.ptr(), self.len)
        }
    }
}
// Deref to enable []- Syntax to write to values in the array. ex: 'arr[1]="hello";'
impl<T> std::ops::DerefMut for Array<T> {
    fn deref_mut(&mut self) -> &mut [T] {
        unsafe {
            std::slice::from_raw_parts_mut(self.ptr(), self.len)
        }
    }
}





// Note: 'iter' and 'iter_mut' we got automagically with Deref. But 'into_iter' and 'drain' not.
// - (those consume the Array by-value and yield its elements value by value)
// they get implemented as DoubleEnded Iterator -> with 2 pointers pointing to the ends.
// - 'end' points AFTER the element it wants read next. 'start' points DIRECTLY at the element. (for clarity when done)
pub struct IntoIter<T> {
    _buf: RawArray<T>,
    start: *const T,
    end: *const T,
}

// because IntoIter takes ownership it NEEDS to Drop to free it. (ex. elements that were not yielded)
impl<T> Drop for IntoIter<T> {
    fn drop(&mut self) {
        // only need to ensure all our elements get droped, RawArray buffer will clean up after itself
        for _ in &mut *self {}
    }
}

impl<T> IntoIterator for Array<T> {
    type Item = T;
    type IntoIter = IntoIter<T>;

    fn into_iter(self) -> IntoIter<T> {
        unsafe {
            // need to use ptr::read to unsafely move the buf out (and we cant destructure it)
            let buf = std::ptr::read(&self.buf);
            let len = self.len;
            mem::forget(self);


            IntoIter {
                start: buf.ptr.as_ptr(),
                end: if buf.cap ==0 {
                    buf.ptr.as_ptr()    // we can only offset/add off the pointer if something is allocated
                } else {
                    buf.ptr.as_ptr().add(len)
                },
                _buf: buf,
            }
        }
    }

}

impl<T> Iterator for IntoIter<T> {
    type Item = T;
    /// consumes next element in array - read out what next points to then increment next.
    fn next(&mut self) -> Option<T> {
        if self.start == self.end {
            None    // iterator is empty
        } else {
            unsafe {
                let result = std::ptr::read(self.start);
                self.start = self.start.offset(1);
                Some(result)
            }
        }
    }

    fn size_hint(&self) -> (usize, Option<usize>) {
        let len = (self.end as usize - self.start as usize) / mem::size_of::<T>();
        (len, Some(len))
    }
}

impl<T> DoubleEndedIterator for IntoIter<T> {
    /// consumes last element from array - end points to after what we want to read -> so we offset first then read 
    fn next_back(&mut self) -> Option<Self::Item> {
        if self.start == self.end {
            None    // iterator is empty
        } else {
            unsafe {
                self.end = self.end.offset(-1);
                let result = std::ptr::read(self.end);
                Some (result)
            }
        }
    }
}

// the pointer (and capacity) get abstracted out from Array.
// - this way all the unsafe memory allocation gets decoupled/gathered in one spot.
#[derive(Debug)]
struct RawArray<T> {
    ptr: NonNull<T>,
    cap: usize,
}
// by implementing Send/Sync ontop of the NonNull<T> pointer we get the same benefits of Unique<T>

unsafe impl <T: Send> Send for RawArray<T> {}
unsafe impl <T: Sync> Sync for RawArray<T> {}

impl<T> RawArray<T> {
    fn new() -> Self {
        assert!(mem::size_of::<T>() !=0, "TODO: implement Zero size allocation");
        RawArray { 
            ptr: NonNull::dangling(), // like Unique<T> a wrapper over a raw pointer. (that cant be NULL and must be of type T)
            cap: 0, 
        }
    }
    /// grow will take care of all the memory allocation happening
    /// - will double size once capacity is reached
    /// - if out of memory-error it will call system implemention to abort
    fn grow(&mut self) {
        const INITIALCAP:usize = 8;     // first time we grow above initial len=cap=0 we skipp to this.
        let (new_cap, new_layout) = if self.cap==0 {
            (INITIALCAP, Layout::array::<T>(INITIALCAP).unwrap()) // since len=cap=0 we need to start with some bigger value
        } else {
            let new_cap = 2 * self.cap;
            let new_layout = Layout::array::<T>(new_cap).unwrap();
            (new_cap, new_layout)
        };
        // ensure that the new allocation is inside what a 32 or 64bit system pointer can handle
        // staying inside those sizes and the above Layouts shouldn never unwrap
        assert!(new_layout.size() <= isize::MAX as usize, "Allocation to large");

        // depending if cap was 0 previous we first allocate or reallocate memory with bigger cap:
        let new_ptr = if self.cap == 0 {
            unsafe { std::alloc::alloc(new_layout) }
        } else {
            let old_layout = Layout::array::<T>(self.cap).unwrap();
            let old_ptr = self.ptr.as_ptr() as * mut u8;
            unsafe { std::alloc::realloc(old_ptr, old_layout, new_layout.size()) }
        };

        // if allocation fails (not enough free memory) new_ptr will be null -> we abort (not just exit)
        self.ptr = match NonNull::new(new_ptr as *mut T) {
            Some(p) =>p,
            None => std::alloc::handle_alloc_error(new_layout), // calls system default no-memory-abort
        };
        self.cap = new_cap;
    }
}

impl <T> Drop for RawArray<T> {
    /// Drop trait - destructor that cleans up -> in this case deallocates our memory
    /// - if self.cap == 0 no allocation happened and we skip deallocation
    fn drop(&mut self) {
        if self.cap != 0 {
            let layout = Layout::array::<T>(self.cap).unwrap();
            unsafe{
                std::alloc::dealloc(self.ptr.as_ptr() as *mut u8, layout);
            }
        }
    }
}
