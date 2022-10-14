# SDF2HandPose

Proof-of-concept project showing usage of SDF 3D texture as input for VR Hand Posing.
Currently it only works using editor buttons on SDFHand script just to show off.

Run SampleScene, position entire hand object over 3D object (any kind, with any collider, with layer==Grabbable),
tap Close Hand button in SDFHand script.

![SDF2HandPose](https://user-images.githubusercontent.com/2834098/183507555-11756449-e3fa-4969-9877-a653fc98e9f4.gif)

**Known issues/future improvements:**
* Closing of hand will not work if Game Tab is not visible
* Finger closing stops when finger's pad is near SDF surface, it could also check other parts of finger for better posing
* SDF don't need to be updated every frame - it can be used once just before the hand closing
* SDF baking/slicking/etc can be probably done in Coroutine and split between several WaitForEndOfFrame to not dip down performance
* maybe use UniTask instead of Coroutines

