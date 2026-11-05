// ควบคุมความปลอดภัยโดยอนุญาตเฉพาะ endpoint จัดการเนื้อหาหลักสูตรและคลังข้อสอบตามสัญญา
export function allowedContentRoute(method: string, path: string): boolean {
  return (
    (method === "GET" && (/^(curricula|question-banks)(\/\d+)?$/.test(path) || path === "levels")) ||
    (method === "POST" && /^(curricula|question-banks)(\/\d+\/publish)?$/.test(path)) ||
    (method === "POST" && /^curricula\/\d+\/(import|approve-criteria)$/.test(path)) ||
    (method === "PUT" && /^(curricula|question-banks)\/\d+$/.test(path)) ||
    (method === "PUT" && /^curricula\/\d+\/content$/.test(path))
  );
}
