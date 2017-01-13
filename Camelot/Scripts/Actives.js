$(function () {
    // Reference the hub.  
    var hubNotif = $.connection.sessionMini;
    // Start the connection.  
    $.connection.hub.start().done(function () {
        getActives();
    });
    // Notify while anyChanges.  
    hubNotif.client.updateActives = function () {
        getActives();
    };
});
function getActives() {
    var model = $('#activesTable');
    $.ajax(
    {
        url: '/home/GetAllActives',
        contentType: 'application/html ; charset:utf-8',
        type: 'GET',
        dataType: 'html',
        success: function (result) {
            model.empty().append(result);
        },
        error: function (e) {
            alert(e);
        }
    });
}