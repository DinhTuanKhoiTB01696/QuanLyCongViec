export function filenameFromResponse(response, fallback = 'SprintA-export.csv') {
  const disposition = response?.headers?.['content-disposition'] || response?.headers?.get?.('content-disposition') || ''
  const utf8 = disposition.match(/filename\*=UTF-8''([^;]+)/i)
  const ascii = disposition.match(/filename="?([^";]+)"?/i)
  try { return decodeURIComponent(utf8?.[1] || ascii?.[1] || fallback) } catch { return ascii?.[1] || fallback }
}

export function downloadResponseFile(response, fallback = 'SprintA-export.csv', defaultType = 'application/octet-stream') {
  const data = response?.data
  if (!data || typeof data === 'object' && !(data instanceof Blob) && !(data instanceof ArrayBuffer)) throw new Error('Phản hồi tải tệp không hợp lệ.')
  const blob = data instanceof Blob ? data : new Blob([data], { type: response?.headers?.['content-type'] || defaultType })
  const href = URL.createObjectURL(blob); const link = document.createElement('a')
  link.href = href; link.download = filenameFromResponse(response, fallback); link.click()
  setTimeout(() => URL.revokeObjectURL(href), 1000)
  return link.download
}

export function csvWithBom(rows) {
  const escape = (value) => `"${String(value ?? '').replace(/"/g, '""')}"`
  return `\uFEFF${rows.map((row) => row.map(escape).join(',')).join('\r\n')}`
}
