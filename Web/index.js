"use strict";
const express = require('express');
const app = express();
const port = 8081;

app.get('/', (req, res) => {
    res.statusCode = 200;
    res.send('Ok');
});

app.listen(port, () => {
    console.log(`Server listen on port ${port}`);
});