var communicationHub;

$(function () {
    communicationHub = $.connection.communicationHub;

    communicationHub.client.updateProgress = function (number) {
        if (number > vm.ProgressBar.progress()) {
            vm.ProgressBar.progress(number);
        }
    };

    communicationHub.client.HandleSuccess = function () {
        vm.ProgressBar.progress(100);
        vm.ProgressBar.state(ProgressStateTypes.Success);
        vm.notificationHub.PushNotification(new SuccessNotification('Congrats!', 'Your database pseudonymized successfully!'));
    };

    communicationHub.client.HandleFailed = function () {
        vm.ProgressBar.state(ProgressStateTypes.Failed);
        vm.notificationHub.PushNotification(new ErrorNotification('Error', 'Something went wrong...'));
    };

    $.connection.hub.start().done(function () {
        communicationHub.server.connect();
    });
});