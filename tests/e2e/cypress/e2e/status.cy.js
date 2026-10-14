describe('Status page', () => {
  beforeEach(() => {
    cy.visit('/status');
  });

 it('shows API OK and DB OK', () => {
  cy.get('[data-cy="api-status"]')
    .should('be.visible')
    .and('contain', 'Healthy');

  cy.get('[data-cy="db-status"]')
    .should('be.visible')
    .and('contain', 'Healthy');
});
});