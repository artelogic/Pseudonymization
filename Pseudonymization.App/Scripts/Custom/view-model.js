var ProgressStateTypes = {
    Regular: {
        Name: 'regular',
        Color: '#4e73df'
    },
    Failed: {
        Name: 'failed',
        Color: '#e74a3b'
    },
    Success: {
        Name: 'success',
        Color: '#1cc88a'
    }
}

var vm = {
    ProgressBar: {
        progress: ko.observable(0),
        state: ko.observable(ProgressStateTypes.Regular)
    },
    connectionStringInput: ko.observable(''),
    messageBox: new modalComponent('err-window', this),
    tables: ko.observableArray([]),
    onProcessBtnClicked: function (data, event) {
        onProcessBtnClickedEventHandler(this);
    },
    onPseudonymizeBtnClicked: function (data, event) {
        onPseudonymizeBtnClickedEventHanlder(this);
    },
    connectionId: ko.observable(''),
    providerName: ko.observable(''),
    notificationHub: new NotificationHub(this)
};

function onPseudonymizeBtnClickedEventHanlder(model) {
    model.ProgressBar.progress(0);
    model.ProgressBar.state(ProgressStateTypes.Regular);
    
    var schemaArray = [];

    ko.utils.arrayForEach(model.tables(), function (tbl) {
        var index = schemaArray.findIndex((s) => s.SchemaName === tbl.Schema);

        var tblToPush = tbl.IsIncluded()
            ? {
                Name: tbl.Name,
                Columns: []
            }
            : null;

        if (tblToPush) {
            tblToPush.Columns = ko.toJS(ko.utils.arrayFilter(tbl.Columns(), function (col) {
                return col.IsIncluded();
            }));

            if (tblToPush.Columns.length === 0) {
                tblToPush = null;
            }
        }

        if (index === -1) {
            var newItem = { SchemaName: tbl.Schema, Tables: [] };

            if (tblToPush) {
                newItem.Tables.push(tblToPush);
            }

            schemaArray.push(newItem);
        } else {
            if (tblToPush) {
                schemaArray[index].Tables.push(tblToPush);
            }
        }
    });

    var tblCount = 0;
    ko.utils.arrayForEach(schemaArray, (s) => {
        tblCount += s.Tables.length;
    });

    if (tblCount === 0) {
        model.messageBox.show('Error!', 'Choose table or column!');
    } else {
        $.ajax({
            type: 'POST',
            url: 'Api/RunPseudonymization',
            data: {
                connectionString: model.connectionStringInput(),
                providerName: model.providerName(),
                schemaList: schemaArray,
                connectionToken: model.connectionId()
            },
            success: function (data) {
                model.notificationHub.PushNotification(new Notification('Congrats!', 'Your tasks has been enqueued.'));
            },
            error: function (error) {
                model.notificationHub.PushNotification(new ErrorNotification('Error', 'Something went wrong'));
                console.log(error);
            }
        });
    }
}


function onProcessBtnClickedEventHandler(model) {
    model.ProgressBar.progress(0);
    model.ProgressBar.state(ProgressStateTypes.Regular);

    if (model.connectionStringInput() === null || model.connectionStringInput() === '') {
        model.messageBox.show('Error', 'Empty connection string!');
    } else {
        $.ajax({
            type: 'POST',
            url: 'Api/ProcessConnectionString',
            data: {
                connectionString: model.connectionStringInput()
            },
            success: function (data) {
                model.tables([]);
                model.connectionId(data.connectionToken);
                model.providerName(data.providerName);
                communicationHub.server.changeToken(data.connectionToken);

                ko.utils.arrayForEach(data.schemas, function (sch) {
                    ko.utils.arrayForEach(sch.Tables, function (tbl) {
                        model.tables.push(new DbTable(tbl, sch.SchemaName, this));
                    });
                });

                model.notificationHub.PushNotification(new Notification('Success!', 'Schema analysis result ready.'));
            },
            error: function (error) {
                model.notificationHub.PushNotification(new ErrorNotification('Error', 'Something went wrong'));
                console.log(error);
            }
        });
    }
}

function modalComponent(id, model) {
    var self = this;

    self.title = ko.observable('');
    self.message = ko.observable('');

    self.show = function (title, message) {
        self.title(title);
        self.message(message);
        $('#' + id).modal('show');
    };

    return self;
}


// not signalr hub!
function NotificationHub(model) {
    var self = this;

    self.NotificationIdentityCounter = 0;
    self.NotificationStack = ko.observableArray([]);

    self.PushNotification = function (notification) {
        // remove outdated notifications
        self.NotificationStack.remove(ntf => ntf.Active === false);
        notification.Identity('notification-' + ++self.NotificationIdentityCounter);
        self.NotificationStack.push(notification);
        $('.toast').toast('show');
    };

    return self;
}