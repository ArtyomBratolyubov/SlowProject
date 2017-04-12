var tasksManager = function () {
    

    // appends a row to the tasks table.
    // @parentSelector: selector to append a row to.
    // @obj: task object to append.
    var appendRow = function (parentSelector, obj) {
        var tr = $("<tr data-id='" + obj.ToDoId + "'></tr>");
        tr.append("<td class='text-center'><input type='checkbox' id='checkInpt-" + obj.ToDoId + "' class='completed' " + (obj.IsCompleted ? "checked" : "") + "/></td>");
        tr.append("<td class='name' >" + obj.Name + "</td>");
        tr.append("<td><input type='button'  id='delBtn-" + obj.ToDoId + "' class='delete-button btn bg-danger ' value='Delete' /></td>");
        $(parentSelector).append(tr);
    }

    // adds all tasks as rows (deletes all rows before).
    // @parentSelector: selector to append a row to.
    // @tasks: array of tasks to append.
    var displayTasks = function (parentSelector, tasks) {
        $(parentSelector).empty();
        $.each(tasks, function (i, item) {
            appendRow(parentSelector, item);
        });
    };

    // starts loading tasks from server.
    // @returns a promise.
    var loadTasks = function () {
        return $.getJSON("/api/todos",hideSpinner);
    };

    // starts creating a task on the server.
    // @isCompleted: indicates if new task should be completed.
    // @name: name of new task.
    // @return a promise.
    var createTask = function (isCompleted, name) {
        return $.ajax(
        {
            url: "http://localhost:50433/Data/Create",

            method: "POST",
            crossDomain: true,
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                UserId: $.cookie("user"),
                ToDoId: 0,
                IsCompleted: isCompleted,
                Name: name
            }),

        });
    };

    // starts updating a task on the server.
    // @id: id of the task to update.
    // @isCompleted: indicates if the task should be completed.
    // @name: name of the task.
    // @return a promise.
    var updateTask = function (id, isCompleted, name) {
        return $.ajax(
        {
            url: "http://localhost:50433/Data/Update/",
            type: "POST",
            contentType: 'application/json',
            crossDomain: true,
            data: JSON.stringify({
                ToDoId: id,
                IsCompleted: isCompleted,
                Name: name,
                UserId: $.cookie("user")
            })
        });
    };

    // starts deleting a task on the server.
    // @taskId: id of the task to delete.
    // @return a promise.
    var deleteTask = function (taskId) {
        return $.ajax({
            url: "http://localhost:50433/Data/Delete/",
            type: "POST",
            crossDomain: true,
            contentType: "application/json",
            data: JSON.stringify({
                ToDoId: taskId,
                UserId: $.cookie("user")
            })

        });
    };

    var syncTask = function (taskId) {
        return $.ajax({
            url: "http://localhost:50433/Data/SyncObj/",
            type: "POST",
            crossDomain: true,
            contentType: "application/json",
            data: JSON.stringify({
                ToDoId: taskId
            }),
            success: function () {

            }

        });
    };

    // returns public interface of task manager.
    return {
        loadTasks: loadTasks,
        displayTasks: displayTasks,
        createTask: createTask,
        deleteTask: deleteTask,
        updateTask: updateTask,
        syncTask: syncTask
    };
}();


$(function () {
    // add new task button click handler
    $("#newCreate").click(function () {
        var isCompleted = $('#newCompleted')[0].checked;
        var name = $('#newName')[0].value;

        if (!name)
            return;
        if (name.length > 50)
            return;

        $('#newCompleted')[0].checked = null;
        $('#newName')[0].value = null;
        var id;
        tasksManager.createTask(isCompleted, name)
            .done(function (data) {
                id = data;
            })
            .then(tasksManager.loadTasks,showSpinner)
            .done(function (tasks) {
                tasksManager.displayTasks("#tasks > tbody", tasks);

                $("#delBtn-" + id).addClass("disabled");
                $("#checkInpt-" + id).addClass("disabled");
                tasksManager.syncTask(id)
                    .done(function () {
                        $("#delBtn-" + id).removeClass("disabled");
                        $("#checkInpt-" + id).removeClass("disabled");
                    });
            });
    });

    // bind update task checkbox click handler
    $("#tasks > tbody").on('change', '.completed', function () {
        var tr = $(this).parent().parent();
        var taskId = tr.attr("data-id");
        var isCompleted = tr.find('.completed')[0].checked;
        var name = tr.find('.name').text();
        showSpinner();
        tasksManager.updateTask(taskId, isCompleted, name)
            .then(tasksManager.loadTasks)
            .done(function (tasks) {
                tasksManager.displayTasks("#tasks > tbody", tasks);
                hideSpinner();
            });
    });

    // bind delete button click for future rows
    $('#tasks > tbody').on('click', '.delete-button', function () {
        if ($(this).hasClass("disabled"))
            return;

        var taskId = $(this).parent().parent().attr("data-id");

        showSpinner();
        tasksManager.deleteTask(taskId)
            .then(tasksManager.loadTasks)
            .done(function (tasks) {
                tasksManager.displayTasks("#tasks > tbody", tasks);
                hideSpinner();
            });
    });

    // load all tasks on startup
    tasksManager.loadTasks()
        .done(function (tasks) {
            tasksManager.displayTasks("#tasks > tbody", tasks);
        });
});

var showSpinner=function () {
    $('#spinner').show();
}

var hideSpinner=function () {
    $('#spinner').hide();
}