const fs = require('fs');

let tl = fs.readFileSync('Frontend/src/views/HomeSite/Teams/TeamList.vue', 'utf8');

// Find table in TeamList
let startStr = '<div class="table-container"';
let startIdx = tl.indexOf(startStr);

if (startIdx !== -1) {
  let tableHtml = tl.substring(startIdx, tl.indexOf('</template>'));
  // Remove the trailing tags (approximate, usually closing divs for the container)
  tableHtml = tableHtml.replace(/<\/div>\s*<\/div>\s*<\/div>\s*$/, '</div>');
  tableHtml = tableHtml.replace(/<\/div>\s*<\/div>\s*$/, '</div>');
  
  let td = fs.readFileSync('Frontend/src/views/HomeSite/Teams/TeamsDashboard.vue', 'utf8');
  let gridStart = td.indexOf('<div class="team-cards-grid" v-if="viewMode === \'grid\'">');
  
  if (gridStart !== -1) {
    let sectionEnd = td.indexOf('</section>', gridStart);
    let before = td.substring(0, sectionEnd);
    let after = td.substring(sectionEnd);
    
    // Replace variable reference
    tableHtml = tableHtml.replace(/filteredTeams/g, 'teams');
    
    // Avoid double injection
    if (!td.includes('<table class="jira-table"')) {
      td = before + '\n\n          ' + tableHtml + '\n        ' + after;
      
      let cssMatch = tl.match(/\/\* Jira Table \*\/[\s\S]*?(?=\<\/style\>)/);
      if (cssMatch && !td.includes('/* Jira Table */')) {
        td = td.replace('</style>', cssMatch[0] + '\n</style>');
      }
      
      fs.writeFileSync('Frontend/src/views/HomeSite/Teams/TeamsDashboard.vue', td);
      console.log('Injected table successfully');
    } else {
      console.log('Table already exists in TeamsDashboard');
    }
  } else {
    console.log('Grid start not found');
  }
} else {
  console.log('Table not found in TeamList');
}

// -------------------------------------------------------------
// Now fix the ProjectList popup filtering "true" instead of "Đang chờ xử lý"
let pList = fs.readFileSync('Frontend/src/views/HomeSite/Projects/ProjectList.vue', 'utf8');

// We need to filter out 'true' and 'false' from statusOptions since they are probably boolean values mixed in
// Let's replace statusOptions definition:
pList = pList.replace(/const statusOptions = computed\(\(\) => \{[\s\S]*?\}\)/, `const statusOptions = computed(() => {
  const statuses = projectStore.projects?.map(p => p.status) || []
  const validStatuses = [...new Set(statuses)].filter(s => 
    s !== null && 
    s !== undefined && 
    s !== true && 
    s !== false && 
    s !== 'true' && 
    s !== 'false' &&
    s.toString().trim() !== ''
  )
  return Array.from(new Set(validStatuses.map(translateStatus)))
})`);

fs.writeFileSync('Frontend/src/views/HomeSite/Projects/ProjectList.vue', pList);
console.log('Fixed ProjectList status options');
