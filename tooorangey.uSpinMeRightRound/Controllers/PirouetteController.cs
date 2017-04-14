using ImageProcessor;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Umbraco.Core.IO;
using Umbraco.Web.WebApi;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Web.Models;
using System.Globalization;
using Umbraco.Core.Logging;
using System.IO;
using ImageProcessor.Imaging.Formats;
using System.Drawing.Imaging;
using ImageProcessor.Imaging;
using tooorangey.uSpinMeRightRound.Models;

namespace tooorangey.uSpinMeRightRound.Controllers
{
    public class PirouetteController : UmbracoAuthorizedApiController
    {
          [HttpPost]
        public IHttpActionResult RotateMedia(RotateInstruction rotateInstruction)
        {

            var mediaId = rotateInstruction.MediaId;
            var turns = rotateInstruction.Turns;
            var mediaService = Services.MediaService;
            var mediaFileSystem = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();

            // get mediaItem
            var mediaItem = mediaService.GetById(mediaId);
            if (mediaItem == null)
            {
              return BadRequest(string.Format("Couldn't find the media item to rotate"));
            }
            var umbracoFile = mediaItem.GetValue<string>("umbracoFile");
            if (String.IsNullOrEmpty(umbracoFile))
            {
                return BadRequest(string.Format("Couldn't retrieve the umbraco file details of the item to rotate"));
            }
            var filePath = String.Empty;

            if (umbracoFile.DetectIsJson())
            {
                var imageCrops = new ImageCropDataSet();
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
            else
            {
                filePath = umbracoFile;
            }
            if (String.IsNullOrEmpty(filePath))
            {               
                return BadRequest("Path to media Item not found");
            }
            
            string fullPath = mediaFileSystem.GetFullPath(filePath);
            var fileName = mediaFileSystem.GetFileName(filePath);
            var rollBackFileName = fileName.Replace(".", "_original.");
            var newFileName = fileName.Replace(".", "_rotated" + turns.ToString() + ".");
            using (ImageFactory imageFactory = new ImageFactory(false))
            {  
                    try
                    {
                    // imageFactory.AnimationProcessMode = ImageProcessor.Imaging.AnimationProcessMode.All;
                    //large files 2000x3000 run out of memory here, when creating the 'copy' (to ensure image is in the most efficient format
                    // Image formatted = this.Image.Copy(this.AnimationProcessMode);
                    // essentially withing the copy this bit runs out of memory
                    // Create a new image and copy it's pixels.
                    //Bitmap copy = new Bitmap(source.Width, source.Height, format);
                    // and error is parameter is not valid
                    var imageToRotate = imageFactory.Load(fullPath);
                    if (rotateInstruction.CreateNewMediaItem)
                    {
                        var ms = new MemoryStream();
                        imageToRotate.Rotate(turns * 90).Save(ms);
                        ms.Position = 0;
                  
                        var memoryStreamPostedFile = new MemoryStreamPostedFile(ms, newFileName);
                        //var newFilePath = fullPath.Replace(fileName, newFileName);
                        var newMediaItem = mediaService.CreateMediaWithIdentity(mediaItem.Name + "_rotated" + (turns * 90).ToString(), mediaItem.ParentId, "Image");
                        newMediaItem.SetValue("umbracoFile", memoryStreamPostedFile);
                        mediaService.Save(newMediaItem);                    
                        ms.Dispose();
                        memoryStreamPostedFile.DisposeIfDisposable();
                        imageFactory.DisposeIfDisposable();
                        return Ok(newMediaItem.Id);
                    }
                    else
                    {
                        var ms = new MemoryStream();
                        imageToRotate.Rotate(turns * 90).Save(ms);
                        ms.Position = 0;
                        //var newFilePath = fullPath.Replace(fileName, newFileName);
                        mediaFileSystem.AddFile(fullPath, ms, true);
                        ms.Dispose();

                        if (turns == 1 || turns == 3)
                        {
                      
                            mediaItem.SetValue("umbracoWidth", imageToRotate.Image.Width);
                            mediaItem.SetValue("umbracoHeight", imageToRotate.Image.Height);
                            mediaService.Save(mediaItem);                        
                        }
                        imageFactory.DisposeIfDisposable();
                    }
                        
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(typeof(PirouetteController), "Error rotating image", ex);
                        return BadRequest(ex.Message);
                    }
                }
                return Ok();
            }
    }

    public class RotateInstruction
    {
        public int MediaId { get; set; }
        public int Turns { get; set; }
        public bool CreateNewMediaItem { get; set; }
    }

   
}
