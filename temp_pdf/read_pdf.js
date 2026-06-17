const fs = require('fs');
const pdf = require('pdf-parse');

let dataBuffer = fs.readFileSync(String.raw`c:\Users\tua46\OneDrive\Máy tính\DATN\QuanLyCongViec\Tổng hợp chức năng tích hợp.pdf`);

pdf(dataBuffer).then(function(data) {
    console.log(data.text);
}).catch(err => {
    console.error("Error parsing PDF:", err);
});
