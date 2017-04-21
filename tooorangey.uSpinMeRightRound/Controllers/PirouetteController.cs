using ImageProcessor;
using System;
using System.Web.Http;
using Umbraco.Core.IO;
using Umbraco.Web.WebApi;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Web.Models;
using System.Globalization;
using Umbraco.Core.Logging;
using System.IO;
using tooorangey.uSpinMeRightRound.Models;

namespace tooorangey.uSpinMeRightRound.Controllers
{
    public class PirouetteController : UmbracoAuthorizedApiController
    {
        /// <summary>
        /// The api endpoint to do the rotation or not and update the underlying umbraco media item or create a new one
        /// </summary>
        /// <param name="rotateInstruction">object containing id of media item and number of turns to rotate and whether to create a new media item</param>
        /// <returns></returns>
          [HttpPost]
        public IHttpActionResult RotateMedia(RotateInstruction rotateInstruction)
        {
            var mediaId = rotateInstruction.MediaId;
            var turns = rotateInstruction.Turns;
            var mediaService = Services.MediaService;
            var mediaFileSystem = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            var isCropper = false;
            var imageCrops = new ImageCropDataSet();
            // get mediaItem
            var mediaItem = mediaService.GetById(mediaId);
            var mediaItemAlias = mediaItem.ContentType.Alias;
            if (mediaItem == null)
            {
              return BadRequest(string.Format("Couldn't find the media item to rotate"));
            }
            var umbracoFile = mediaItem.GetValue<string>("umbracoFile");
            if (String.IsNullOrEmpty(umbracoFile))
            {
                return BadRequest(string.Format("Couldn't retrieve the umbraco file details of the item to rotate"));
            }

            //read in the filepath from umbracoFile
            var filePath = String.Empty;
            if (umbracoFile.DetectIsJson())
            {                
                isCropper = true;
                try
                {
                    imageCrops = JsonConvert.DeserializeObject<ImageCropDataSet>(umbracoFile, new JsonSerializerSettings
                    {
                        Culture = CultureInfo.InvariantCulture,
                        FloatParseHandling = FloatParseHandling.Decimal
                    });

                }
                catch (Exception ex)
                {
                    LogHelper.Error(typeof(PirouetteController), "Could not parse the json string: " + umbracoFile, ex);
                }
                filePath = imageCrops.Src;
            }
            else // not cropper
            {
                filePath = umbracoFile;
            }
            if (String.IsNullOrEmpty(filePath))
            {               
                return BadRequest("Path to media Item not found");
            }
            // use mediaFileSystem to get path
            string fullPath = mediaFileSystem.GetFullPath(filePath);
            var fileName = mediaFileSystem.GetFileName(filePath);
            // get new filename based on complicated rules of how much it's been rotated :-)
            // creating a new filename for the rotation should avoid file locks when saving the image
            var newFileName = GetNewRotatedFileName(fileName, turns);
            // determine if a file already exists with our new rotated name in the same path we're going to save it in
            var rotatedFileExists = mediaFileSystem.FileExists(fullPath.Replace(fileName, newFileName));
            using (ImageFactory imageFactory = new ImageFactory(false))
            {  
                    try
                    {                    
                    if (rotateInstruction.CreateNewMediaItem)
                    {
                        // checkbox to create a new media item for the rotation was checked
                        // so lets rotate the file, converted to a 'PostedFile'
                        // and save to a new Umbraco Media Item
                        var imageToRotate = imageFactory.Load(fullPath);
                        var ms = new MemoryStream();
                        imageToRotate.Rotate(turns * 90).Save(ms);
                        ms.Position = 0;                  
                        var memoryStreamPostedFile = new MemoryStreamPostedFile(ms, newFileName);
                        var newMediaItem = mediaService.CreateMediaWithIdentity(mediaItem.Name + "_rotated" + (turns * 90).ToString(), mediaItem.ParentId, mediaItemAlias);
                        newMediaItem.SetValue("umbracoFile", memoryStreamPostedFile);
                        mediaService.Save(newMediaItem);                    
                        ms.Dispose();
                        // return new media id
                        return Ok(newMediaItem.Id);
                    }
                    else
                    {
                        // rotate the existing media item
                        var newFilePath = fullPath.Replace(fileName, newFileName);
                        //only need to rotate and save the file if it doesn't already exist
                        if (!rotatedFileExists)
                        { 
                            //lets rotate and save the file
                            var imageToRotate = imageFactory.Load(fullPath);
                            var ms = new MemoryStream();
                            imageToRotate.Rotate(turns * 90).Save(ms);
                            ms.Position = 0;                     
                            //overwrite the file if it already exists, though we checked this earlier
                            // use mediaFileSystem Add File to do the saving
                            mediaFileSystem.AddFile(newFilePath, ms, true);
                            ms.Dispose();
                            // an odd number of turns requires us to switch width and height
                            if (turns == 1 || turns == 3)
                            {
                                mediaItem.SetValue("umbracoWidth", imageToRotate.Image.Width);
                                mediaItem.SetValue("umbracoHeight", imageToRotate.Image.Height);
                            }
                        }                       
                        //we have a different url for our media - update underlying mediaitem to point to new file
                        if (isCropper)
                        {
                            imageCrops.Src = imageCrops.Src.Replace(fileName, newFileName);
                            mediaItem.SetValue("umbracoFile", JsonConvert.SerializeObject(imageCrops));
                        }
                        else // not cropper
                        {
                            mediaItem.SetValue("umbracoFile", filePath.Replace(fileName, newFileName));
                        }              
                        //update the media item
                        mediaService.Save(mediaItem); 
                    }
                        
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(typeof(PirouetteController), "Error rotating image", ex);
                        return BadRequest(ex.Message);
                    }
                }
                return Ok(mediaItem.Id);
            }
        private string GetNewRotatedFileName(string fileName, int turns)
        {
            //if we've rotated this image before the filename will already have -rotated tacked on the end
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var fileExtension = Path.GetExtension(fileName);
            // what if image has been rotated once, it will end -rotated1 if we rotate another 90 we want that to be rotate2 not rotate1 again to avoid having the same filename, trying to be cleverer here
           // this will screw up if filename is called fish-rotated1-rotated2.jpg but that'll never happen?
           int prevTurns = 0;
           if (fileNameWithoutExtension.Length > 8)
            {
                var lastEightCharacters = fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 9);
               switch (lastEightCharacters)
                {
                    case "-rotated0":
                    case "-rotated1":
                    case "-rotated2":
                    case "-rotated3":
                        Int32.TryParse(fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 1), out prevTurns);
                        fileNameWithoutExtension = fileNameWithoutExtension.Remove(fileNameWithoutExtension.IndexOf("-rotated"), 9);
                        break;

                    default:
                        //do nothing
                        break;

                }

            }      
            //now lets add -rotated to the filename and the number of turns and any prevTurns
            int turnsFromSource = (turns + prevTurns) % 4;
            if (turnsFromSource > 0){
                return fileNameWithoutExtension + "-rotated" + turnsFromSource.ToString() + fileExtension;
            }
            else
            { // we've rotated 360 don't need the word rotated in the filename...
                return fileNameWithoutExtension + fileExtension;
            }
        }
    }
    
    public class RotateInstruction
    {
        public int MediaId { get; set; }
        public int Turns { get; set; }
        public bool CreateNewMediaItem { get; set; }
    }

   
}
