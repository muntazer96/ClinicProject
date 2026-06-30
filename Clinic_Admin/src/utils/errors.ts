export function getErrorMessage(error: any, fallback = 'تعذر تنفيذ العملية. حاول مرة أخرى.') {
  return error.response?.data?.message ?? error.message ?? fallback
}
