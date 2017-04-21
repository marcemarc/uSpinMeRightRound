angular.module("umbraco").controller("tooorangey.SelectRotationController", function ($scope, $http, entityResource, mediaHelper, navigationService, notificationsService, $location) {
    
    var vm = this;

    vm.mediaInfo = {
        filePath:'',
        imageName:'',
        mediaId: 0
    };

    vm.status = {
        hasImage: false,
        resize: 'width=200',
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
        //console.log(mediaEntity);
        vm.mediaInfo.imageName = mediaEntity.name;
        if (mediaEntity.metaData.umbracoHeight.Value > mediaEntity.metaData.umbracoWidth.Value){
            
            vm.status.resize = 'height=225'
        }
        var mediaFile = mediaHelper.resolveFileFromEntity(mediaEntity, false);
            //console.log(mediaFile);
            vm.mediaInfo.filePath = mediaFile;

    });
  
    // write out rotated versions of the images
    // sends selected rotation and media id to api controller
    // that does the rotation, saves the rotated image in media item or new media item
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
                   // console.log(response);
                    // close the slide out box
                    navigationService.hideDialog();
         
                    // reload the media node
                    if (currentMediaItem.id != response.data) {
                        $location.path('media/media/edit/' + response.data);
                    }
                    else {
                        window.location.reload(true);
                    }
              
                }, function (response) {
                    //console.log(response);
                    //notify errors
                    navigationService.hideDialog();
                    notificationsService.remove(0);
                    notificationsService.error("Error Rotating Image", response.data.Message);

                });
        
    }

});