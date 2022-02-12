
# Ropuz Puzzle

Is a puzzle game that users can create and share their own puzzles.
Users can select a photo from their galery and convert it to a puzzle and ask for his/her friends to solve the puzzle.

## Game Play Video
https://www.youtube.com/watch?v=qne1ZK4XZfE

## Screen Shots
### Jigsaw Puzzle
![Before](https://user-images.githubusercontent.com/20722654/153708348-876c7f4d-b3f1-4c43-bbd5-2e5bb336bfa0.png)
![After](https://user-images.githubusercontent.com/20722654/153708639-4bd69c57-c7b5-4206-af80-2ca617686cb0.PNG)

### Slider Puzzle
![Before2](https://user-images.githubusercontent.com/20722654/153708587-fed5fa9e-29d3-4686-9de4-658178137237.png)
![After2](https://user-images.githubusercontent.com/20722654/153708594-8849559a-2bd9-4bd1-96d7-cf0e2e4014ef.png)

### Rotate Puzzle
![Before 3](https://user-images.githubusercontent.com/20722654/153708681-a8b410be-7bd5-44a5-a18e-24ea00bc8bdf.png)
![After 3](https://user-images.githubusercontent.com/20722654/153708689-21233c47-30ae-4dc8-a73c-c32b3bef8aa5.png)

### Custom Level
![Custom-1](https://user-images.githubusercontent.com/20722654/153708839-7b5cb2c1-3319-41bf-a984-867b97ce1010.png)
![Custom-2](https://user-images.githubusercontent.com/20722654/153708841-4657a016-9282-4552-a579-2eaec2dc5e34.png)

## Installation
### APK Installation
You can install the Ropuz.apk files in Apks folder to your mobile device.
### Unity Installation
- Install Firebase Storage, Firebase Push Notification and Deep Linking.
- Install Admob
- Install Native Galery plugin.
    
## Used Technologies

**Unity3D :** Game development environment

**Firebase :** Storage, Push Notification, Deep Linking

**Admob :** Reward, Interstitial, Banner

**Opensource :** NativeGallery

## Experienced
I designed a puzzle game for the first time with three diffrent types of puzzle with new firebase services.

### Puzzle Game
#### Jigsaw Puzzle
User were just trying to drag and drop the jigsaw puzzle items to their position to complete the level.
#### Slider Puzzle
Users were sliding the empty place to fit all the image pieces their own positions.
#### Rotate Puzzle
There were a rotate button at the center of four image piece to let users rotate these four items around each other.
Users were trying to complete the level with rotating these grid items.

### Firabase Services
I use Firebase Push Notifications and Firebase Analytics for almost every project.
But in the puzzle project i had to use more. 
Firebase Dynamic Links and Firebase Storage service.
#### Firebase Storage 
After user build a level with the selected image from his/her galery.
I upload the image to the Firebase storage with a configuration file.
This configuration file contains the informations of the custom level.
#### Firebase Dynamic Links
After files loaded successfully to the Firebase Storage i create a short link to let creator share the level with his/her friends.
But when i do this, i was just letting user to share the link with only a text. 
So i also needed to attach the image of the level with a blured effect to the share link.
#### Native share
Untiy Native Share plugin to let users share images via whatsapp.
I just attach the image to the link and let users to share their levels with images.
    
## Special Thanks

- [@yasirkula](https://github.com/yasirkula/) native galery plugin.



  
