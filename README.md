# SDF2HandPose

Proof-of-concept project showing usage of SDF 3D texture as input for VR Hand Posing.
Currently it only works using editor buttons on SDFHand script just to show off.
It can be compared with physics based hand posing using PhysicsHand script.

## Citation

This code is a part of my work **Autograsping pose of virtual hand model using the Signed Distance Field real-time sampling with fine-tuning**.
If you want to use it please consider citing:
```
@article{Puchalski2023,title = {Autograsping pose of virtual hand model using the Signed Distance Field real-time sampling with fine-tuning},journal = {Computer Science Research Notes},year = {2023},volume = {31},number = {1-2},pages = {232-240},author = {Puchalski, M. and Woźna-Szcześniak, B.}} 
```

## Running

Run SampleScene, position entire hand object over 3D rigidbody (any kind, with any collider, with layer==Grabbable),
tap **Close Hand** button in SDFHand script. **Toggle interactive update** will enable continous autograsping pose.

**Fine tune** option will turn on finetuning of the grasp using last SDF sample.

Turning on Animator component will enable animation of hand moving on the spine of Dragon Model - if turned
on with **Toggle interactive update** will result in dragon petting :)

**Work in Progress** SDFMagnet script allows to test reach-to-grasp using SDF - requires that target mesh MeshToSDF
set with bigger Flood steps number

![Nice dragon](https://media.githubusercontent.com/media/nosferathoo/SDF2HandPose/main/DragonPetting2.gif)

**Update 23.01.2023**
* code cleanup
* SDFMagnet - reach-to-grasp work in progress script
* Additional SDF sampler with normals calculation using Tetrahedron technique

**Update 19.01.2023**
* added additional SIGGRAPH's Pixel model
* added finetuning of grasp using last SDF sample 

**Updates 9.12.2022**
* project now uses Mesh-To-SDF unity package for fast SDF computation ( https://github.com/Unity-Technologies/com.unity.demoteam.mesh-to-sdf )
* Finger tips' possible positions mapped to SDF texture coords are cached
* SDF is no longer sliced to create Texture3D from RenderTexture instead it's sampled along all fingers possible positions during bending straight on GPU using ComputeShader
* Sampled data is retrieved from GPU using async call and processed when ready to not stall the app
* added additional meshes for testing (Stanford Armadillo, Stanford Dragon, Utah teapot)

**Known issues/future improvements:**
* ~~Closing of hand will not work if Game Tab is not visible~~
* Finger closing stops when finger's ~~pad~~ tip is near SDF surface, it could also check other parts of finger for better posing
* SDF don't need to be updated every frame - it can be used once just before the hand closing
* ~~SDF baking/slicing/etc can be probably done in Coroutine and split between several WaitForEndOfFrame to not dip down performance~~
* Mesh-To-SDF can be probably done when needed not on every frame
* Mesh-To-SDF has big impact on performance for large meshes with more than 10k faces (e.g. armadillo and dragon)

