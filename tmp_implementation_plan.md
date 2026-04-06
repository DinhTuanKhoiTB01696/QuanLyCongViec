# Implementation Plan: Space Summary Dashboard Update

## Objective
Implement logic calculations for the top 4 widgets, redesign the "Loại hình công việc" (Types of Work) section into a customized progress-bar layout, and modify the "Tổng quan trạng thái" (Status overview) to feature an interactive custom text legend showing percentages.

## 1. Top 4 Widgets Logic
Add computed variables in `<script setup>`:
- `completedTasksLast7Days`: Filter `tasks.value` where `statusName === 'DONE'` and `updatedAt` (or `dueDate`) is within the last 7 days.
- `updatedTasksLast7Days`: Filter `tasks.value` where `updatedAt` is within the last 7 days.
- `createdTasksLast7Days`: Filter `tasks.value` where `createdAt` is within the last 7 days.
- `dueSoonTasksNext7Days`: Filter `tasks.value` where `dueDate` is >= today and <= today + 7 days.

Replace the static `$0` text in the template with these computed variables.

## 2. Loại hình công việc (Types of Work) UI Redesign
Currently it uses ECharts (`id="type-pie-chart"`). Remove the ECharts instance for this specific chart.
Replace the UI with a custom HTML/CSS layout matching Image 1:
- A list of task types: Nhiệm vụ (Task), Sử thi (Epic), Câu chuyện (Story), Nhiệm vụ phụ (Subtask).
- Calculate total active items (tasks + subtasks + epics ...). For now, `tasks.value` holds all of them, differentiated by `typeName` (Task, Epic, Story, Subtask).
- Calculate percentages: (count of type / total valid items) * 100.
- Render horizontal progress bars with labels.

## 3. Tổng quan trạng thái (Status Overview) Redesign
Keep the ECharts donut chart, but disable the built-in legend (`legend: { show: false }`).
Create a custom HTML legend box next to the chart.
- The legend box will list "Đã hoàn thành: X", "Cần làm: Y", "Đang thực hiện: Z".
- Add `@mouseenter`/`@mouseleave` to show a tooltip detailing percentage and counts, exactly as requested ("Đã hoàn thành: 2 (Hoàn thành trong vòng 2 tuần qua)").
- For the Echarts Donut, add a graphic text in the center showing the total valid items or largest percentage (like "33% Done" in Image 2).

## Next Steps
1. Update `<script setup>` logic (computeds and layout vars).
2. Update `<template>` layout for charts (removing Echart type-pie-chart, adding HTML variants).
3. Update `<style scoped>` for new elements.
