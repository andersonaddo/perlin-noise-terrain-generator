# Perlin Noise 3D Terrain Generator
This is a terrain generator based off Perlin Noise. The core of it was made using Videos 1-9 of [Sebastian Lague's Video Series on Procedural Landmass Generation](https://www.youtube.com/watch?v=wbpMiKiSKm8&list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3). 
It utilizes multithreading.

Here's a quick summary of how it generated infinite terrain:

 - The terrain is split into several chunks called `TerrainChunks`. These chunks are only classes—they are not explicitly linked to any gameobject. They only represent regions in space.
 - A chunk will be created when the viewer gets close enough to it's respective region. Upon creation, a chink will request a for a package of information packaged in a `mapData` class from the `Noise` class. This information essentially contains just a noise map and a color map based off this noise map and the currently active biome.
 - A Texture is created from this `mapData`
 - When needed, the chunk will request for a package of information packaged in a `meshData` class, which contains a mesh based off it's `mapData` and it's biome. Meshes are created with certain levels of detail for optimization (meshes that are further away from the player contain less polys). This means that any chunk can contain several `meshData` instances, and use any particular one based of it's distance form the viewer.
 - This mesh is then given the Texture of it's  `mapData` and assigned to a physical gameObject, and placed in the scene. Little pieces of details called `artifacts` (like trees or cacti) are placed randomly on the mesh. References to these artifacts are stored in their respective `TerrainChunks`
 -  Once it gets far enough from the player, it is made inactive and it's physical gameObejct is pooled.
 
 Some modifications have been made to this software that were not made in the tutorial. Namely:
 
 - Artifacts were added. 
 - The generation is biome based. Biomes are scriptableObjects with lot's of options and aesthetic variables (including scene post-processing) that guide the noise, color, texture and mesh generation.
 - A custom editor was creates to make the color definitions of biomes easy to manipulate.
 - Object pooling was implimented into the gameObjects allocated to TerrainChunks. Initially, each TerrainChunk was given a permanent gameObject, whether they were active or not.
 
Here's a video of the generator in action (artifacts hadn't been added yet, though):
[[embed url=https://www.youtube.com/watch?v=nnDUjDxTEuk]]
# Perlin Noise 3D Terrain Generator
This is a terrain generator based off Perlin Noise. The core of it was made using Videos 1-9 of [Sebastian Lague's Video Series on Procedural Landmass Generation.](https://www.youtube.com/watch?v=wbpMiKiSKm8&list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3)
It utilizes multithreading.

Here's a quick summary of how it generates infinite terrain:

 - The terrain is split into several chunks called `TerrainChunks`. These chunks are only classes—they are not explicitly linked to any gameobject. They only represent regions in space.
 - A chunk will be created when the viewer gets close enough to it's respective region. Upon creation, a chink will request a for a package of information packaged in a `mapData` class from the `Noise` class. This information essentially contains just a noise map and a color map based off this noise map and the currently active biome.
 - A Texture is created from this `mapData`
 - When needed, the chunk will request for a package of information packaged in a `meshData` class, which contains a mesh based off it's `mapData` and it's biome. Meshes are created with certain levels of detail for optimization (meshes that are further away from the player contain less polys). This means that any chunk can contain several `meshData` instances, and use any particular one based of it's distance form the viewer.
 - This mesh is then given the Texture of its  `mapData` and assigned to a physical gameObject, and placed in the scene. Little pieces of details called `artifacts` (like trees or cacti) are placed randomly on the mesh. References to these artifacts are stored in their respective `TerrainChunks`
 - Once the chunk is far enough from the player, it is made inactive and it's physical gameObejct is pooled.
 
 Some modifications have been made to this software that were not made in the tutorial. Namely:
 
 - Artifacts were added. 
 - The generation is biome based. Biomes are scriptableObjects with lot's of options and aesthetic variables (including scene post-processing) that guide the noise, color, texture and mesh generation.
 - A custom editor was creates to make the color definitions of biomes easy to manipulate.
 - Object pooling was implimented into the gameObjects allocated to TerrainChunks. Initially, each TerrainChunk was given a permanent gameObject, whether they were active or not.
 
Here's a video of the generator in action (artifacts hadn't been added yet, though):
[![Alt text](https://img.youtube.com/vi/nnDUjDxTEuk/0.jpg)](https://www.youtube.com/watch?v=nnDUjDxTEuk)


> Written with [StackEdit](https://stackedit.io/).