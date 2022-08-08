# SDF2HandPose
# SDF2HandPose

Proof-of-concept project showing usage of SDF 3D texture as input for VR Hand Posing.
Currently it only works using editor buttons on SDFHand script just to show off.

Run SampleScene, position entire hand object over 3D object (any kind, with any collider, with layer==Grabbable),
tap Close Hand button in SDFHand script.



**Known issues/future improvements:**
* Closing of hand will not work if Game Tab is not visible
* SDF don't need to be updated every frame - it can be used once just before the hand closing
* SDF baking/slicking/etc can be probably done in Coroutine and split between several WaitForEndOfFrame to not dip down performance
* maybe use UniTask instead of Coroutines
