const fs = require('fs');

let tl = fs.readFileSync('Frontend/src/views/HomeSite/Teams/TeamList.vue', 'utf8');
let td = fs.readFileSync('Frontend/src/views/HomeSite/Teams/TeamsDashboard.vue', 'utf8');

let tableStart = tl.indexOf('<table v-if="viewMode === \'table\'" class="jira-table">');
if (tableStart !== -1) {
  let tableEnd = tl.indexOf('</table>', tableStart);
  let tableHtml = tl.substring(tableStart, tableEnd + 8);
  
  // Need to replace filteredTeams with teams because TeamsDashboard uses `teams` directly
  tableHtml = tableHtml.replace(/filteredTeams/g, 'teams');
  
  let gridStart = td.indexOf('<div class="team-cards-grid" v-if="viewMode === \'grid\'">');
  if (gridStart !== -1) {
    // End of grid is after the empty state
    let gridEnd = td.indexOf('</section>', gridStart);
    let before = td.substring(0, gridEnd);
    let after = td.substring(gridEnd);
    
    // Make sure we didn't inject it before
    td = before + '\n\n          ' + tableHtml + '\n        ' + after;
    
    // Clean up if we accidentally injected double (which we didn't because it failed last time)
    let cssMatch = tl.match(/\/\* Jira Table \*\/[\s\S]*?(?=\<\/style\>)/);
    if (cssMatch && !td.includes('/* Jira Table */')) {
      td = td.replace('</style>', cssMatch[0] + '\n</style>');
    }
    
    fs.writeFileSync('Frontend/src/views/HomeSite/Teams/TeamsDashboard.vue', td);
    console.log('Injected table successfully');
  } else {
    console.log('Grid start not found');
  }
} else {
  console.log('Table not found');
}
