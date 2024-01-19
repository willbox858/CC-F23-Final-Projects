In this project, I wanted to experiment with using the GPU for broad-phase collision-detection.

To this end, I calculated a Z-Order space-filling curve on the GPU, a 
structure that mimics the way that an Octree subdivides space while also
being ideal for parallel computation due to the bottom up nature of how it is constructed.

There are two scenes included with this project, one named "CPU_Octree_TestScene" which 
contains a demo scene used to test using an Octree for broad-phase collision detection.

While in this scene, press "C" on the keyboard to enable or disable octree for broad-phase testing.

There is also a scene labelled "GPU_Z-Order-SFC_TestScene, which contains a demo scene 
where a Z-Order curve is calculated and used to determine if two particles should be tested for collision.

The base assets and unit testing code for this project were provided by the professor. 
The implementation of the Octree and Z-Order SFC compute shader were both written by me. 

references used for this project.

https://www.cse.iitb.ac.in/~rhushabh/publications/i3D/I3D08Poster.pdf
https://developer.nvidia.com/blog/thinking-parallel-part-i-collision-detection-gpu/
https://developer.nvidia.com/blog/thinking-parallel-part-ii-tree-traversal-gpu/
https://developer.nvidia.com/blog/thinking-parallel-part-iii-tree-construction-gpu/
