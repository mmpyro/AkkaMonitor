"use strict";
const express = require('express');
const app = express();
const port = 8081;

const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

app.get('/', async (req, res) => {
    await sleep(500);
    res.status(200).send('Ok');

});

app.listen(port, () => {
    console.log(`Server listen on port ${port}`);
});