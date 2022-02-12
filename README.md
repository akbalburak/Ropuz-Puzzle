
# Ropuz Puzzle

Is a puzzle game that users can create and share their own puzzles.
Users can select a photo from their galery and convert it to a puzzle and ask for his/her friends to solve the puzzle.


## Used Technologies

**Unity3D** Game developmen environment

**Firebase :** Storage, Push Notification, Deep Linking

**Admob :** Reward, Interstitial, Banner

**Opensource :** NativeGallery

## Experienced

I designed a puzzle game for the first time with three diffrent types of puzzle.

### Jigsaw Puzzle
User were just trying to drag and drop the jigsaw puzzle items to their position to complete the level.
### Slider Puzzle
Users were sliding the empty place to fit all the image pieces their own positions.
### Rotate Puzzle
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
Thanks for - [@yasirkula](https://github.com/yasirkula/) for Untiy Native Share plugin.
I just attach the image to the link and let users to share their levels with images.
  
## Special Thanks

- [@yasirkula](https://github.com/yasirkula/) native galery plugin.

  
## Ekran Görüntüleri

![Uygulama Ekran Görüntüsü]()

  