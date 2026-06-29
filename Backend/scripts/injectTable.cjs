const fs = require('fs');
let tl = fs.readFileSync('Frontend/src/views/HomeSite/Teams/TeamList.vue', 'utf8');
let td = fs.readFileSync('Frontend/src/views/HomeSite/Teams/TeamsDashboard.vue', 'utf8');

// The table in TeamList is inside <div class="table-container" v-if="viewMode === 'table'">
let startIdx = tl.indexOf('<div class="table-container" v-if="viewMode === \'table\'">');
if (startIdx !== -1) {
  let endIdx = tl.indexOf('</div>\n      </div>', startIdx); // This is approximate, let's find the end of table-container manually
  if (endIdx === -1) endIdx = tl.indexOf('</template>');
  
  // Actually let's just substring up to the next div closing the wrapper
  let tableHtml = tl.substring(startIdx, tl.indexOf('</template>', startIdx));
  // Remove trailing closing tags that belong to the template
  tableHtml = tableHtml.replace(/<\/div>\s*<\/div>\s*<\/div>\s*$/, '</div></div>');
  
  // In TeamsDashboard, insert after <div class="team-cards-grid" v-if="viewMode === 'grid'">...</div>
  let gridStart = td.indexOf('<div class="team-cards-grid" v-if="viewMode === \'grid\'">');
  let nextSection = td.indexOf('</section>', gridStart);
  
  if (gridStart !== -1 && nextSection !== -1) {
    let beforeGridEnd = td.substring(0, nextSection);
    let afterGridEnd = td.substring(nextSection);
    
    // tableHtml needs to use 'teams' instead of 'filteredTeams'
    // But in our previous script we might have already used filteredTeams, so let's just replace both just in case
    tableHtml = tableHtml.replace(/filteredTeams/g, 'teams');
    
    td = beforeGridEnd + '\n\n          ' + tableHtml + '\n        ' + afterGridEnd;
    
    // Add Jira Table CSS if not present
    let cssMatch = tl.match(/\/\* Jira Table \*\/[\s\S]*?(?=\<\/style\>)/);
    if (cssMatch && !td.includes('/* Jira Table */')) {
      td = td.replace('</style>', cssMatch[0] + '\n</style>');
    }
    
    fs.writeFileSync('Frontend/src/views/HomeSite/Teams/TeamsDashboard.vue', td);
    console.log('Injected table into TeamsDashboard');
  }
}
