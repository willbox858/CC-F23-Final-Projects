This project was my final project for GPR-350 (Game Physics).

In it, I wanted to experiment with GPU accelerated physics. 

To this end, I calculated a Z-Order space-filling-curve (SFC) on the GPU to be used for broad-phase collision detection.
This structure was chosen due to the way it mimics how an Octree subdivides space while also
being ideal for parallel computation due to the bottom up nature of how the SFC is constructed.

The base assets and unit testing code for this project were provided by the professor. 
The implementation of the Octree and Z-Order SFC compute shader were both written by me. 

references used for this project.

https://www.cse.iitb.ac.in/~rhushabh/publications/i3D/I3D08Poster.pdf
https://developer.nvidia.com/blog/thinking-parallel-part-i-collision-detection-gpu/
https://developer.nvidia.com/blog/thinking-parallel-part-ii-tree-traversal-gpu/
https://developer.nvidia.com/blog/thinking-parallel-part-iii-tree-construction-gpu/
