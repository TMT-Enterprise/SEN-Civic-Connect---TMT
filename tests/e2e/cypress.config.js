const { defineConfig } = require('cypress');

module.exports = defineConfig({
  e2e: {
    baseUrl: process.env.CYPRESS_BASE_URL || 'http://localhost:4200',
    specPattern: 'cypress/e2e/**/*.cy.js',
    supportFile: false,
  },
});