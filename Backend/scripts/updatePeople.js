const fs = require('fs');

function updatePeopleDirectory() {
    const file = 'Frontend/src/views/HomeSite/People/PeopleDirectory.vue';
    let content = fs.readFileSync(file, 'utf-8');

    // Add import
    if (!content.includes('DropdownFilter.vue')) {
        content = content.replace("import UserAvatar from '@/components/common/UserAvatar.vue'", 
            "import UserAvatar from '@/components/common/UserAvatar.vue'\nimport DropdownFilter from '@/components/common/DropdownFilter.vue'");
    }

    // Replace selects with DropdownFilter
    content = content.replace(/<select class="filter-chip" v-model="filters\.projectId">[\s\S]*?<\/select>/, 
        `<DropdownFilter label="Dự án" :options="projectOptions" v-model="filters.projectId" />`);

    content = content.replace(/<select class="filter-chip" v-model="filters\.goalId">[\s\S]*?<\/select>/, 
        `<DropdownFilter label="Mục tiêu" :options="goalOptions" v-model="filters.goalId" />`);

    content = content.replace(/<select class="filter-chip" v-model="filters\.teamId">[\s\S]*?<\/select>/, 
        `<DropdownFilter label="Nhóm" :options="teamOptions" v-model="filters.teamId" />`);

    content = content.replace(/<select class="filter-chip" v-model="filters\.managerId">[\s\S]*?<\/select>/, 
        `<DropdownFilter label="Người quản lý" :options="managerOptions" v-model="filters.managerId" />`);

    content = content.replace(/<select class="filter-chip" v-model="filters\.jobTitle">[\s\S]*?<\/select>/, 
        `<DropdownFilter label="Chức danh" :options="jobTitleOptions" v-model="filters.jobTitle" />`);

    content = content.replace(/<select class="filter-chip" v-model="filters\.department">[\s\S]*?<\/select>/, 
        `<DropdownFilter label="Phòng ban" :options="departmentOptions" v-model="filters.department" />`);

    content = content.replace(/<select class="filter-chip" v-model="filters\.location">[\s\S]*?<\/select>/, 
        `<DropdownFilter label="Vị trí" :options="locationOptions" v-model="filters.location" />`);

    fs.writeFileSync(file, content);
    console.log('PeopleDirectory.vue updated');
}

updatePeopleDirectory();
