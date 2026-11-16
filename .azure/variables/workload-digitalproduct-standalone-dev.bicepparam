using '../templates/workload-digitalproduct.bicep'

// Phase 1 (mode 1, standalone): plan, Log Analytics, App Insights, web, API and SQL all live in this RG.
// RG: {product}-{env}-{region}-{instance}-rg. Set SQL_ENTRA_ADMIN_GROUP_OBJECT_ID and AZURE_TENANT_ID to workload-tenant IDs before deployment.
// Deploy: az deployment group create -g agentstart-dev-wus2-100-rg -f .azure/templates/workload-digitalproduct.bicep -p .azure/variables/workload-digitalproduct-standalone-dev.bicepparam

var productIac = 'agentstart'
var environmentIac = 'dev'
var regionIac = 'wus2'
var instanceIac = '100'
var prefix = '${productIac}-${environmentIac}-${regionIac}-${instanceIac}'

param mode = 'standalone'
param location = 'westus2'
param environmentApp = 'Development'
param tags = { Environment: environmentIac, CostCenter: '0000' }

param hosting = {
  appiName: '${prefix}-appi'
  planName: '${prefix}-F1-asp'
  planSku: 'F1'
  workspaceName: '${prefix}-log'
}

// Components: delete a line to skip that component.
param web = { name: '${prefix}-web' }
param api = { name: '${prefix}-api', userAssignedIdentityName: '${prefix}-api-id' }
param sql = {
  serverName: '${prefix}-sql'
  databaseName: '${prefix}-sqldb'
  databaseSku: 'Basic'
  server: {
    kind: 'EntraOnly'
    administrator: {
      login: 'Workload SQL administrators'
      objectId: readEnvironmentVariable('SQL_ENTRA_ADMIN_GROUP_OBJECT_ID', '00000000-0000-0000-0000-000000000000')
      tenantId: readEnvironmentVariable('AZURE_TENANT_ID', '00000000-0000-0000-0000-000000000000')
    }
  }
}
