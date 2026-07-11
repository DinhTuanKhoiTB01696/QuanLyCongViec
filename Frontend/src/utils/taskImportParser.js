/**
 * taskImportParser.js
 * Utility for parsing CSV/XLSX files and validating task rows for import.
 */
import * as XLSX from 'xlsx'

// ────────────────────────────────────────────
// Header mapping (Vietnamese + English)
// ────────────────────────────────────────────
const HEADER_MAP = {
  // Vietnamese
  'tiêu đề công việc': 'title',
  'tiêu đề': 'title',
  'tên công việc': 'title',
  'mô tả': 'description',
  'email người phụ trách': 'assigneeEmail',
  'người phụ trách': 'assigneeEmail',
  'trạng thái': 'status',
  'ưu tiên': 'priority',
  'hạn hoàn thành': 'dueDate',
  'hạn': 'dueDate',
  'story point': 'storyPoints',
  'story points': 'storyPoints',
  'điểm': 'storyPoints',
  'ước lượng giờ': 'estimatedHours',
  // English
  'title': 'title',
  'description': 'description',
  'assigneeemail': 'assigneeEmail',
  'assignee email': 'assigneeEmail',
  'assignee': 'assigneeEmail',
  'status': 'status',
  'priority': 'priority',
  'duedate': 'dueDate',
  'due date': 'dueDate',
  'due': 'dueDate',
  'storypoint': 'storyPoints',
  'storypoints': 'storyPoints',
  'story_points': 'storyPoints',
  'estimatedhours': 'estimatedHours',
  'estimated hours': 'estimatedHours',
  'hours': 'estimatedHours',
}

/**
 * Map raw header strings to standardized field names.
 */
export function mapHeaders(rawHeaders) {
  return rawHeaders.map(h => {
    const normalized = (h || '').trim().toLowerCase()
    return HEADER_MAP[normalized] || null
  })
}

// ────────────────────────────────────────────
// CSV Parser (RFC 4180 compliant)
// ────────────────────────────────────────────
export function parseCSVText(text) {
  const lines = []
  let row = ['']
  let inQuotes = false
  for (let i = 0; i < text.length; i++) {
    const c = text[i]
    const next = text[i + 1]
    if (c === '"') {
      if (inQuotes && next === '"') {
        row[row.length - 1] += '"'
        i++
      } else {
        inQuotes = !inQuotes
      }
    } else if (c === ',' && !inQuotes) {
      row.push('')
    } else if ((c === '\r' || c === '\n') && !inQuotes) {
      if (c === '\r' && next === '\n') i++
      lines.push(row)
      row = ['']
    } else {
      row[row.length - 1] += c
    }
  }
  if (row.length > 1 || row[0] !== '') {
    lines.push(row)
  }
  return lines
}

// ────────────────────────────────────────────
// XLSX Parser
// ────────────────────────────────────────────
export function parseXLSXBuffer(arrayBuffer) {
  const workbook = XLSX.read(arrayBuffer, { type: 'array' })
  const firstSheetName = workbook.SheetNames[0]
  if (!firstSheetName) return []
  const worksheet = workbook.Sheets[firstSheetName]
  // Convert to array of arrays (header row included)
  return XLSX.utils.sheet_to_json(worksheet, { header: 1, defval: '' })
}

// ────────────────────────────────────────────
// Priority normalizer
// ────────────────────────────────────────────
const PRIORITY_MAP = {
  'urgent': 1, 'khẩn cấp': 1, '1': 1,
  'high': 2, 'cao': 2, '2': 2,
  'medium': 3, 'trung bình': 3, 'normal': 3, 'bình thường': 3, '3': 3,
  'low': 4, 'thấp': 4, '4': 4,
}

function normalizePriority(raw) {
  if (!raw && raw !== 0) return 3 // default Medium
  const key = String(raw).trim().toLowerCase()
  return PRIORITY_MAP[key] ?? 3
}

// ────────────────────────────────────────────
// Validate & build rows
// ────────────────────────────────────────────
/**
 * Process raw 2D array (including header row) into validated task rows.
 * Returns { rows, validCount, invalidCount }
 */
export function processRawRows(rawRows, projectMembers = [], projectStatuses = []) {
  if (!rawRows || rawRows.length < 2) {
    return { rows: [], validCount: 0, invalidCount: 0 }
  }

  const headerRow = rawRows[0]
  const fieldMap = mapHeaders(headerRow)

  // Check if at least "title" column exists
  if (!fieldMap.includes('title')) {
    return { rows: [], validCount: 0, invalidCount: 0, headerError: 'Không tìm thấy cột "Tiêu đề công việc" hoặc "title" trong header.' }
  }

  const emailSet = new Set(
    (projectMembers || []).map(m => m.email?.toLowerCase().trim()).filter(Boolean)
  )
  const statusSet = new Set(
    (projectStatuses || []).map(s => s.name?.toUpperCase().trim()).filter(Boolean)
  )

  const rows = []
  let validCount = 0
  let invalidCount = 0

  for (let i = 1; i < rawRows.length; i++) {
    const cols = rawRows[i]
    // Skip completely empty rows
    const hasContent = cols.some(c => String(c || '').trim() !== '')
    if (!hasContent) continue

    const record = {
      rowNum: i + 1,
      title: '',
      description: '',
      status: '',
      assigneeEmail: '',
      priority: 3,
      storyPoints: '',
      dueDate: '',
      estimatedHours: '',
      error: null,
      isChecked: true,
    }

    // Map columns to fields
    fieldMap.forEach((field, colIdx) => {
      if (field && cols[colIdx] !== undefined) {
        const val = String(cols[colIdx]).trim()
        if (field === 'priority') {
          record.priority = normalizePriority(val)
        } else {
          record[field] = val
        }
      }
    })

    // Validate
    if (!record.title) {
      record.error = 'Thiếu tiêu đề công việc'
      record.isChecked = false
    } else if (record.assigneeEmail && !emailSet.has(record.assigneeEmail.toLowerCase())) {
      record.error = 'Email người phụ trách không thuộc dự án'
      record.isChecked = false
    } else if (record.dueDate && isNaN(Date.parse(record.dueDate))) {
      record.error = 'Hạn hoàn thành không đúng định dạng (VD: YYYY-MM-DD)'
      record.isChecked = false
    } else if (record.status && statusSet.size > 0 && !statusSet.has(record.status.toUpperCase())) {
      // Fallback status to BACKLOG
      record.status = 'BACKLOG'
    }

    if (record.error) {
      invalidCount++
    } else {
      validCount++
    }
    rows.push(record)
  }

  return { rows, validCount, invalidCount }
}

/**
 * Build API payload for a validated row.
 */
export function buildTaskPayload(row, projectMembers = []) {
  const assignee = (projectMembers || []).find(
    m => m.email?.toLowerCase() === row.assigneeEmail?.toLowerCase()
  )
  return {
    title: row.title.trim(),
    description: row.description || '',
    statusName: row.status || 'TO DO',
    priority: row.priority || 3,
    // CreateWorkTaskDto uses non-nullable numeric fields. Sending null makes
    // ASP.NET model binding reject every imported row with HTTP 400.
    storyPoints: row.storyPoints ? Number(row.storyPoints) : 0,
    dueDate: row.dueDate || null,
    totalEstimatedHours: row.estimatedHours ? Number(row.estimatedHours) : 0,
    assignedUserId: assignee?.userId || assignee?.id || null,
    assigneeIds: assignee ? [assignee.userId || assignee.id] : [],
  }
}
