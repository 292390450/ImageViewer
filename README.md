# ImageViewer
a wpf ImageViewer

#use 
Install-Package ImageViewerZDL -Version 1.0.1

sampleUse 
```
 var images = new ImageViewWindow(new string[]{ @"C:\Users\zeng\Pictures\d21.jpg"} );
 images.Show();
```
or Pass in the object list, and the return path of the given func

```
    var images = new ImageViewWindow();
            images.Show();
            images.SetSource<string>(new string[] { @"C:\Users\zeng\Pictures\d21.jpg",@"C:\Users\zeng\Pictures\78801ec615ee48fa8399263ef7b3c0a5.jpg"}
          , (ob) =>
                 {
                     return ob;
                 }, 99);
                 
```
