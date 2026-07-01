const fs = require('fs');
const path = require('path');

const viewsDir = path.join(__dirname, 'views');

function processFile(filePath) {
    let content = fs.readFileSync(filePath, 'utf-8');
    
    // Check if file has NexusLayout
    if (!content.includes('<NexusLayout')) return;

    // Replace <NexusLayout class="profile-page"> with <div class="profile-page">
    // Replace <NexusLayout> with <div>
    content = content.replace(/<NexusLayout(.*?)>/g, (match, p1) => {
        if (p1.trim()) {
            return `<div${p1}>`;
        }
        return `<div>`;
    });

    // Replace </NexusLayout> with </div>
    content = content.replace(/<\/NexusLayout>/g, '</div>');

    // Remove import NexusLayout from ...
    content = content.replace(/import\s+NexusLayout\s+from\s+['"].*?NexusLayout\.vue['"];?\n?/g, '');
    
    fs.writeFileSync(filePath, content, 'utf-8');
    console.log('Refactored:', filePath);
}

function walkDir(dir) {
    const files = fs.readdirSync(dir);
    for (const file of files) {
        const fullPath = path.join(dir, file);
        if (fs.statSync(fullPath).isDirectory()) {
            walkDir(fullPath);
        } else if (fullPath.endsWith('.vue')) {
            processFile(fullPath);
        }
    }
}

walkDir(viewsDir);
console.log('Done refactoring views.');
