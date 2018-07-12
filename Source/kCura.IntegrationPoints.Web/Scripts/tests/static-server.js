const StaticServer = require('static-server');
const server = new StaticServer({
  rootPath: './..',            // required, the root of the server file tree
  port: 9000,               // required, the port to listen
  name: 'JavaScript-Tests-Server',   // optional, will set "X-Powered-by" HTTP header
  host: '127.0.0.1',       // optional, defaults to any interface
  cors: '*',                // optional, defaults to undefined
  followSymlink: true,      // optional, defaults to a 404 error
  templates: {
    index: 'index.html',      // optional, defaults to 'index.html'
    notFound: '404.html'    // optional, defaults to undefined
  }
});
 
server.start(function () {
  console.log('Server listening to', server.port);
});