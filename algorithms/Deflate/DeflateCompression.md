# Deflate Implementation in Csharp

Working decompressor for single-file gzip-compressed files or Datastreams. 
- Along the DEFLATE Compressed Data Format Specification version 1.3
- as described in **RFC 1951**

##  Usage
```
dotnet run ./compressed-input.gz outputfile.png

>Last modified - 07/08/2023 12.09.15 +00:00
>Compression - maximal Compression and slowest algorithm.
>File-System-Info - value: 0 => FAT filesystem (MS-DOS, OS/2, NT/Win32)
>Flag3 FNAME- Indicating File name: Screenshot 2023-08-07 140912.png
>successfully decompressed outfile.png
```

## useful links

- https://www.rfc-editor.org/rfc/rfc1951.html

- https://www.nayuki.io/page/simple-deflate-decompressor