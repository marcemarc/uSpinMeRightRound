angular.module("umbraco").controller("tooorangey.SelectRotationController", function ($scope, $http, entityResource, mediaHelper, navigationService,notificationsService) {
    
    var vm = this;

    vm.mediaInfo = {
        filePath:'',
        imageName:'',
        mediaId: 0,
        thumbPath: ''
    };

    vm.status = {
        hasImage: false,
        resize: 'width=300',
        selected: 1,
        isSelectionMode: true,
        createNewMediaItem: false
    };

    // get the current media id
    var dialogOptions = $scope.dialogOptions;
    var currentMediaItem = dialogOptions.currentNode;
    vm.mediaInfo.mediaId = parseInt(currentMediaItem.id);
    console.log(currentMediaItem);

    // use entity resource to pull back it's url
    entityResource.getById(currentMediaItem.id, "media").then(function (mediaEntity) {
        console.log(mediaEntity);
        vm.mediaInfo.imageName = mediaEntity.name;
        if (mediaEntity.metaData.umbracoHeight.Value > mediaEntity.metaData.umbracoWidth.Value){
            
            vm.status.resize = 'height=325'
        }
        var mediaFile = mediaHelper.resolveFileFromEntity(mediaEntity, false);
            console.log(mediaFile);
            vm.mediaInfo.filePath = mediaFile;
            vm.mediaInfo.thumbPath = mediaFile.replace(".", "_big - thumb.")
    });
  
    // write out rotated versions of the images
    // clicking the rotation (perhaps use radio button) and button
    // causes loading snake to appear
    // sends selected rotation and media id to api controller
    // that does the rotation, saves the rotated image over the existing
    // updates width and height
    // then reloads the page.

    vm.selectRotation = selectRotation;
    vm.rotate = rotate;
    vm.getDateTime = getDateTime;

    function getDateTime() {
        return new Date();
    }

    function selectRotation(turns) {

        if (turns < 4) {
            vm.status.selected = turns;
        }
    }
    function rotate() {
        vm.status.isSelectionMode = false;
        //hide the slidey out thing, show some animation
        $http.post("/umbraco/backoffice/api/pirouette/rotatemedia", JSON.stringify({ MediaId: parseInt(vm.mediaInfo.mediaId), Turns: vm.status.selected, CreateNewMediaItem: vm.status.createNewMediaItem }),
                {
                    headers: {
                        'Content-Type': 'application/json'
                    }
                }).then(function (response) {
                    //stop animation thing
                    // confirm rotation has occurred
                    // close the slide out box
                    navigationService.hideDialog();
         
                    // reload the page
                    window.location.reload(true);
                }, function (response) {
                    console.log(response);
                    navigationService.hideDialog();
                    notificationsService.remove(0);
                    notificationsService.error("Error Rotating Image", response.data.Message);

                });
        
    }

});