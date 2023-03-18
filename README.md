# AvatarScaleTool

Avatar Scale Tool is an editor script that generates avatar scaling animations for ChilloutVR based on target viewpoint heights. 

The tool is located under `NotAKid/Avatar Scale Tool`.

How to Use
* Select the target avatar.
* Input the desired minimum and maximum heights in meters. The viewpoint position is assumed to be the initial height.
* Modify the Reference Avatar Height to match the height your locomotion animations are rigged for. 
  * This step is optional if you don't plan on using the #MotionScale parameter.
  * The default CCK locomotion animations work well with **1.8m** as the avatar reference height.
* Generate the animation clip and add it to your avatar's animation controller.
  * The generated animation clip is intended to be used with motion time.

Use the **Initial Height Percentage** value to set your *default* slider value in your controller & Avatar Advanced Settings (AAS).

A local float parameter, `#MotionScale`, is animated alongside the root of the avatar. Add this parameter to your controller and use it as the speed modifier for your locomotion animations if you want your walking, crouch, and prone speeds to match your avatar's scaled height.
 
![image](https://user-images.githubusercontent.com/37721153/226088034-7b518420-7f37-4c14-a8c1-ec02b9104931.png)

---

Here is the block of text where I tell you it's not my fault if you're bad at Unity.

> Use of this Unity Script is done so at the user's own risk and the creator cannot be held responsible for any issues arising from its use.

