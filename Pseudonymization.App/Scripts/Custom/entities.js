function DbTable(data, schema, model) {
    var self = this;

    self.IsIncluded = ko.observable(true);

    self.IsIncluded.subscribe(function (newVal) {
        ko.utils.arrayForEach(self.Columns(), function (col) {
            col.IsIncluded(newVal);
        });
    })

    self.Name = data.Name;
    self.Schema = schema;
    self.DisplayName = ko.computed(function () {
        return self.Schema + '.' + self.Name;
    })

    self.Columns = ko.observableArray([]);

    ko.utils.arrayForEach(data.Columns, function (dataElement) {
        self.Columns.push(new DbColumn(dataElement, model));
    });

    return self;
}

function DbColumn(data, table, model) {
    var self = this;

    self.IsIncluded = ko.observable(true);

    self.ColumnName = ko.observable(data.ColumnName);
    self.MaxLength = data.MaxLength;

    return self;
}


function Notification(title, message) {
    var self = this;

    self.Identity = ko.observable('');
    self.Title = ko.observable(title);
    self.Message = ko.observable(message);
    self.Active = true;
    self.Type = NotificationTypes.Info;

    return self;
}

function ErrorNotification(title, message) {
    var notificationBase = new Notification(title, message);
    notificationBase.Type = NotificationTypes.Error;
    return notificationBase;
}

function SuccessNotification(title, message) {
    var notificationBase = new Notification(title, message);
    notificationBase.Type = NotificationTypes.Success;
    return notificationBase;
}