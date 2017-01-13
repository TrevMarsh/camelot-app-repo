$(function () {
    // Reference the hub.  
    var hubNotif = $.connection.sessionMini;
    // Start the connection.  
    $.connection.hub.start().done(function () {
        getAll();
    });
    // Notify while anyChanges.  
    hubNotif.client.updateData = function () {
        getAll();
    };
});
function getAll() {
    var model = $('#dataTable');
    $.ajax(
    {
        url: '/Home/GetAllData',
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
