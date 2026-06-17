const fs = require('fs');
const path = require('path');

function walk(dir) {
    let results = [];
    const list = fs.readdirSync(dir);
    list.forEach(file => {
        file = path.join(dir, file);
        const stat = fs.statSync(file);
        if (stat && stat.isDirectory()) {
            results = results.concat(walk(file));
        } else {
            if (file.endsWith('.vue') || file.endsWith('.css')) {
                results.push(file);
            }
        }
    });
    return results;
}

const files = walk(__dirname);

files.forEach(file => {
    let content = fs.readFileSync(file, 'utf8');
    let changed = false;
    
    // Pattern 1: padding: 8px 12px 8px 32px;
    const pat1 = /padding:\s*8px\s+12px\s+8px\s+32px/g;
    if (pat1.test(content)) {
        content = content.replace(pat1, 'padding: 8px 12px 8px 44px');
        changed = true;
    }
    
    // Pattern 2: padding-left: 36px; (specifically for inline styles or other places I added recently)
    const pat2 = /padding-left:\s*36px/g;
    if (pat2.test(content)) {
        content = content.replace(pat2, 'padding-left: 44px');
        changed = true;
    }

    // Pattern 3: padding-left: 32px; 
    const pat3 = /padding-left:\s*32px/g;
    if (pat3.test(content)) {
        content = content.replace(pat3, 'padding-left: 44px');
        changed = true;
    }

    // Specific to .plane-search-input which might have different padding
    const pat4 = /padding:\s*4px\s+8px\s+4px\s+30px/g;
    if (pat4.test(content)) {
        content = content.replace(pat4, 'padding: 4px 8px 4px 44px');
        changed = true;
    }

    // Specific to nexus-search-input which might have different padding
    const pat5 = /padding:\s*6px\s+12px\s+6px\s+32px/g;
    if (pat5.test(content)) {
        content = content.replace(pat5, 'padding: 6px 12px 6px 44px');
        changed = true;
    }
    
    if (changed) {
        fs.writeFileSync(file, content, 'utf8');
        console.log('Fixed padding in', file);
    }
});
