using '../templates/workload-digitalproduct.bicep'

// Phase 1 (mode 1, standalone): plan, Log Analytics, App Insights, web, API and SQL all live in this RG.
// RG: {product}-{env}-{region}-{instance}-rg. Secrets: set SQL_ADMIN_USER and SQL_ADMIN_PASSWORD in the environment before running az deployment group.
// Deploy: az deployment group create -g agent-dev-wus2-100-rg -f .azure/templates/workload-digitalproduct.bicep -p .azure/variables/workload-digitalproduct-standalone-dev.bicepparam

var productIac = 'agent'
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
  planName: '${prefix}-F1-plan'
  planSku: 'F1'
  workspaceName: '${prefix}-work'
}

// Components: delete a line to skip that component.
param web = { name: '${prefix}-web' }
param api = { name: '${prefix}-api' }
param sql = {
  serverName: '${prefix}-sql'
  databaseName: '${prefix}-sqldb'
  databaseSku: 'Basic'
  adminLogin: readEnvironmentVariable('SQL_ADMIN_USER', 'LocalAdmin')
  adminPassword: readEnvironmentVariable('SQL_ADMIN_PASSWORD', 'PASS_FROM_CLI_PARAMETERS')
}
