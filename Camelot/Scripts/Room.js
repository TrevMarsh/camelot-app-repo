$(function () {
    // reference the hub
    var sessionhub = $.connection.sessionMini;
    // start connection
    $.connection.hub.start().done(function () {
        // do nothing yet and wait for the host/server to post a topic
    });

    // Notify while changes
    sessionhub.client.updateVoteControls = function (round) {
        getControls(round);
    }
});

function getControls(model) {
    // contintue here! use the value of the text label of the user and pass it along to keep a new vm company
    var user = $('#participant').text();

    var section = $('#votingControls');
    var json = JSON.stringify({
        'round': model,
        'user': user
    });
    $.ajax(
    {
        url: '/Voting/GetVotingControls',
        data: json,
        contentType: 'application/json ; charset:utf-8',
        type: 'POST',
        dataType: 'html',
        success: function (result) {
            section.empty().append(result);
        },
        error: function (e) {
            alert(e);
        }
    });
}






