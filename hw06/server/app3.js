var express = require('express');
var mySql = require ('mysql');
var app = express();

var connection = mySql.createConnection({
    host:'localhost',
    user:'admin',
    password:'az7895123',
    database:'gameserver'
});

connection.connect (function(err)
{
    if (err)
    {
        console.log ('Error Connection', err.stack);
        return;
    }
    console.log ('Connected as id', connection.threadId);
});


app.get('/users',function(req,res){
    queryAllUser(function(err,result){
        res.end(result);
    });
});

app.get('/user/:name',function(req,res){
    var name = req.params.name;
    console.log (name);

    queryUser(function(err,result){
        res.end(result);
    },name);
});

app.get('/authen/id',function(req,res){
    var thisUsername = req.query.username;

    checkUsername(thisUsername,function(err,result){
        res.end(result);
    });
});

app.get('/authen/name',function(req,res){
    var thisName = req.query.name;

    checkName(thisName,function(err,result){
        res.end(result);
    });
});

app.get('/login', function (req,res){
    var thisUsername = req.query.username;
    var thisPassword = req.query.password;

    login(thisUsername,thisPassword,function(err,result){
        res.end(result);
    });
});

app.get('/topscore',function(req,res){
    topscore(function(err,result){
        res.end(result);
    })
});

app.get('/onlineauthen',function(req,res){

    var thisUsername = req.query.username;

    sendname(thisUsername,function(err,result)
    {
        res.end(result);
    });

});

app.get('/user/add/user',function(req,res){
    var thisUsername = req.query.username;
    var thisPassword = req.query.password;
    var thisName = req.query.name;
    var thisScore = req.query.score;

    var user = [
        [thisUsername,thisPassword,thisName,thisScore]
    ];

    addUser(user,function(err,result){
        res.end(result);
    })

    //res.end(name + " " + password);
});

var server = app.listen(8081,function(){
    console.log('Server : Running');
});

function queryAllUser (callback)
{
    var json = '';
    connection.query('SELECT * FROM user',
    function (err, rows, fields)
    {
        if (err) throw err;
        
        json = JSON.stringify(rows);

        callback(null,json);
    });
}

function queryUser (callback,name)
{
    var json = '';
    connection.query("SELECT * FROM user WHERE username = ?",name,
    function (err, rows, fields)
    {
        if (err) throw err;
        
        json = JSON.stringify(rows);

        callback(null,json);
    });
}

function topscore (callback)
{
    var json = '';
    var sql = "SELECT name,score FROM user ORDER BY score DESC LIMIT 10";
    connection.query(sql,
    function (err,rows,fields)
    {
        if (err) throw err;

        json = JSON.stringify(rows);

        callback(null,json);
    });
}

function login (username,password,callback)
{
    var sql = "SELECT id,username,password FROM user WHERE username = ? AND password >= ?";
    connection.query(sql,[username,password],
    function(err,rows)
    {
        if (err)
        {
            throw err;
            console.log('fail syntax or internal');
        }

        var userObj = JSON.parse(JSON.stringify(rows));
        var props = Object.keys(userObj);
        for (var key in userObj)
        {
            var user = userObj[key].username
            var pass = userObj[key].password
            var id = userObj[key].id;
        }
        if (username == user)
        {
            if (password == pass)
            {
                var result = '0'
                console.log("ID: " + id + " Login Confirm.");
            }
            else
            {
                var result = '1'
                console.log("User: " + username + " tried to breanch into the system.");
            }
        }
        else
        {
            var result = '1'
            console.log("User: " + username + " tried to breanch into the system.");
        }
        
        callback(null,result);
    });
}

function checkUsername (username,callback)
{
    connection.query("SELECT username FROM user WHERE username = "+connection.escape(username)+" ORDER BY username LIMIT 1",function(err,rows){
        if (err)
        {
            throw err;
            console.log('fail syntax or internal');
        }
        var userObj = JSON.parse(JSON.stringify(rows));
        var props = Object.keys(userObj);
        for (var key in userObj)
        {
            var userSql = userObj[key].username
        }
        if (userSql == username)
        {
            console.log("Account: " + username + " is matches with another");
            var result = '1';
        }
        else
        {
            console.log("Account: " + username + " is allowed");
            var result = '0';
        }
        callback(null,result);
    });
}

function checkName (name,callback)
{
    connection.query("SELECT name FROM user WHERE name = "+connection.escape(name)+" ORDER BY name LIMIT 1",function(err,rows){
        if (err)
        {
            throw err;
            console.log('fail syntax or internal');
        }
        var nameObj = JSON.parse(JSON.stringify(rows));
        var props = Object.keys(nameObj);
        for (var key in nameObj)
        {
            var nameSql = nameObj[key].name
        }
        if (nameSql == name)
        {
            console.log("Name: " + name + " is matches with another");
            var result = '1';
        }
        else
        {
            console.log("Name: " + name + " is allowed");
            var result = '0';
        }
        callback(null,result);
    });
}

function sendname (username,callback)
{
    connection.query("SELECT username,name FROM user WHERE username = " + connection.escape(username) + "",function(err,rows){
        if (err)
        {
            throw err;
            console.log('fail syntax or internal');
        }
        var nameObj = JSON.parse(JSON.stringify(rows));
        var props = Object.keys(nameObj);
        for (var key in nameObj)
        {
            var nameSql = nameObj[key].name
        }

        if (nameSql != "" && nameSql != null)
        {
            console.log("Player: " + nameSql + " online.");
            var result = String(nameSql);
        }
        else
        {
            var result = '1';
        }
        callback(null,result);
    });
}

function addUser (user,callback)
{
    var sql = 'INSERT INTO user(username,password,name,score) values ?';
    connection.query(sql,[user],
    function (err)
    {
        var result = '0'

        if (err)
        {
            var result = '1'
            throw err;
        }

        callback(null,result);
    });
}