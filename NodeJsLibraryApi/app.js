const express = require('express');
const yaml = require('yamljs');
const swaggerUi = require('swagger-ui-express');
const libraryRouter = require('./controllers/libraryController');

const app = express();
const PORT = process.env.PORT || 3010;

// Middleware
app.use(express.json());             // parse JSON bodies
app.use(express.urlencoded({ extended: true }));

// Swagger (OpenAPI) – optional, mirrors `AddOpenApi` / `MapOpenApi`
const swaggerDocument = yaml.load('./openapi.yaml');   // create this file yourself or generate it
app.use('/api-docs', swaggerUi.serve, swaggerUi.setup(swaggerDocument));

// Routes – same base path as you would have with ASP.NET Core controllers
app.use('/api/books', libraryRouter);

// HTTPS redirection – if you run behind a proxy like Nginx you can enable trust‑proxy
if (process.env.NODE_ENV === 'production') {
  app.enable('trust proxy');
  app.use((req, res, next) => {
    if (req.secure) return next();
    res.redirect(`https://${req.headers.host}${req.originalUrl}`);
  });
}

// Start server
app.listen(PORT, () => {
  console.log(`Library API listening on http://localhost:${PORT}`);
});