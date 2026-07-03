import { useState, useEffect, useRef } from 'react';
import { api } from '@/lib/api';

interface Document {
  id: string; documentType: string; displayName: string;
  mimeType: string; fileSize: number; createdAt: string;
}

export default function DocumentsPage() {
  const [docs, setDocs] = useState<Document[]>([]);
  const [uploading, setUploading] = useState(false);
  const fileRef = useRef<HTMLInputElement>(null);

  const load = () => {
    api.get<Document[]>('/documents').then(setDocs).catch(() => {});
  };

  useEffect(() => { load(); }, []);

  const upload = async () => {
    const file = fileRef.current?.files?.[0];
    const type = (document.getElementById('docType') as HTMLSelectElement)?.value;
    if (!file || !type) return;

    setUploading(true);
    try {
      const fd = new FormData();
      fd.append('file', file);
      fd.append('documentType', type);
      await api.upload('/documents', fd);
      load();
      if (fileRef.current) fileRef.current.value = '';
    } catch (err: any) {
      alert(err.message);
    } finally {
      setUploading(false);
    }
  };

  const remove = async (id: string) => {
    try { await api.delete(`/documents/${id}`); load(); }
    catch (err: any) { alert(err.message); }
  };

  const formatSize = (bytes: number) => {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  };

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">Documents</h1>

      <div className="mb-6 rounded-lg border bg-white p-6 shadow-sm">
        <h2 className="mb-3 font-semibold">Upload Document</h2>
        <div className="flex gap-3">
          <select id="docType" className="rounded-md border px-3 py-2 text-sm">
            <option value="Resume">Resume</option>
            <option value="CoverLetter">Cover Letter</option>
          </select>
          <input ref={fileRef} type="file" accept=".pdf,.doc,.docx,.txt,.rtf"
            className="flex-1 rounded-md border px-3 py-2 text-sm" />
          <button onClick={upload} disabled={uploading}
            className="rounded-md bg-primary px-6 py-2 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">
            {uploading ? 'Uploading...' : 'Upload'}
          </button>
        </div>
      </div>

      <div className="space-y-2">
        {docs.map((d) => (
          <div key={d.id} className="flex items-center justify-between rounded-lg border bg-white p-4 shadow-sm">
            <div>
              <p className="font-medium">{d.displayName}</p>
              <p className="text-xs text-muted-foreground">
                {d.documentType} · {d.mimeType} · {formatSize(d.fileSize)} · {new Date(d.createdAt).toLocaleDateString()}
              </p>
            </div>
            <button onClick={() => remove(d.id)}
              className="rounded-md border px-3 py-1 text-xs text-red-600 hover:bg-red-50">Delete</button>
          </div>
        ))}
        {docs.length === 0 && <p className="py-8 text-center text-sm text-muted-foreground">No documents uploaded.</p>}
      </div>
    </div>
  );
}
