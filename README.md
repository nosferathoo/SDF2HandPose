# SDF2HandPose

Proof-of-concept project showing usage of SDF 3D texture as input for VR Hand Posing.
Currently it only works using editor buttons on SDFHand script just to show off.
It can be compared with physics based hand posing using PhysicsHand script.

Run SampleScene, position entire hand object over 3D rigidbody (any kind, with any collider, with layer==Grabbable),
tap Close Hand button in SDFHand script.

![SDF2HandPose](https://user-images.githubusercontent.com/2834098/183507555-11756449-e3fa-4969-9877-a653fc98e9f4.gif)

**Updates 9.12.2022**
* project now uses Mesh-To-SDF unity package for fast SDF probe ( https://github.com/Unity-Technologies/com.unity.demoteam.mesh-to-sdf )
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

