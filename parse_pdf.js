const fs = require('fs');
const pdf = require('pdf-parse');
let dataBuffer = fs.readFileSync('Tổng hợp chức năng tích hợp.pdf');
pdf(dataBuffer).then(function(data) {
  fs.writeFileSync('pdf_extracted.txt', data.text);
  console.log('Done');
});
