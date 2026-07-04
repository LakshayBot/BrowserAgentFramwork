import { useState, useEffect, useRef } from 'react';
import { api } from '@/lib/api';
import { Bug, ChevronDown, ChevronRight, ChevronUp, ImageIcon, Minimize2 } from 'lucide-react';

interface LogEntry {
  id: string;
  timestamp: string;
  level: string;
  stepName?: string;
  message: string;
  data?: string;
  screenshotUrl?: string;
}

interface Props {
  workflowId: string;
  isActive: boolean;
}

export default function WorkflowDebugPanel({ workflowId, isActive }: Props) {
  const [open, setOpen] = useState(false);
  const [logs, setLogs] = useState<LogEntry[]>([]);
  const [expandedJson, setExpandedJson] = useState<Set<string>>(new Set());
  const [expandedScreenshot, setExpandedScreenshot] = useState<string | null>(null);
  const [autoScroll, setAutoScroll] = useState(true);
  const listRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    fetchLogs();
    if (!isActive) return;
    const interval = setInterval(fetchLogs, 3000);
    return () => clearInterval(interval);
  }, [open, workflowId, isActive]);

  useEffect(() => {
    if (autoScroll && listRef.current) {
      listRef.current.scrollTop = listRef.current.scrollHeight;
    }
  }, [logs, autoScroll]);

  const fetchLogs = async () => {
    try {
      const data = await api.get<LogEntry[]>(`/workflows/${workflowId}/logs?limit=200`);
      setLogs(data);
    } catch {}
  };

  const toggleJson = (id: string) => {
    setExpandedJson(prev => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  };

  const levelBadge = (level: string) => {
    const colors: Record<string, string> = {
      Info: 'bg-blue-100 text-blue-700',
      Warning: 'bg-yellow-100 text-yellow-700',
      Error: 'bg-red-100 text-red-700',
      Debug: 'bg-gray-100 text-gray-600',
    };
    return (
      <span className={`rounded px-1.5 py-0.5 text-[10px] font-medium ${colors[level] ?? 'bg-gray-100 text-gray-600'}`}>
        {level}
      </span>
    );
  };

  return (
    <div className="mt-6 rounded-lg border bg-white shadow-sm">
      <button
        onClick={() => setOpen(!open)}
        className="flex w-full items-center justify-between px-4 py-3 text-left hover:bg-gray-50"
      >
        <div className="flex items-center gap-2">
          <Bug className="h-4 w-4 text-muted-foreground" />
          <span className="font-semibold text-sm">Debug Logs</span>
          {logs.length > 0 && (
            <span className="rounded-full bg-gray-100 px-2 py-0.5 text-[10px] text-muted-foreground">
              {logs.length} entries
            </span>
          )}
        </div>
        {open ? <ChevronDown className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
      </button>

      {open && (
        <div className="border-t">
          <div className="flex items-center gap-2 border-b bg-gray-50 px-4 py-1.5">
            <button
              onClick={fetchLogs}
              className="rounded px-2 py-0.5 text-[11px] font-medium text-primary hover:bg-primary/10"
            >
              Refresh
            </button>
            <button
              onClick={() => setAutoScroll(!autoScroll)}
              className={`rounded px-2 py-0.5 text-[11px] font-medium ${autoScroll ? 'bg-primary/10 text-primary' : 'text-muted-foreground hover:bg-gray-100'}`}
            >
              Auto-scroll
            </button>
            {logs.length > 0 && (
              <button
                onClick={() => setLogs([])}
                className="ml-auto rounded px-2 py-0.5 text-[11px] text-muted-foreground hover:bg-gray-100"
              >
                Clear
              </button>
            )}
          </div>

          <div ref={listRef} className="max-h-96 overflow-y-auto">
            {logs.length === 0 ? (
              <p className="p-4 text-center text-xs text-muted-foreground">
                No debug logs yet{isActive ? ' — waiting for workflow to emit logs...' : ''}
              </p>
            ) : (
              <div className="divide-y">
                {logs.map((log) => (
                  <div key={log.id} className="px-4 py-2 text-[12px] leading-relaxed hover:bg-gray-50">
                    <div className="flex items-start gap-2">
                      <span className="mt-0.5 shrink-0 font-mono text-[10px] text-gray-400">
                        {new Date(log.timestamp).toLocaleTimeString()}
                      </span>
                      {levelBadge(log.level)}
                      {log.stepName && (
                        <span className="shrink-0 rounded bg-purple-50 px-1.5 py-0.5 text-[10px] text-purple-700">
                          {log.stepName}
                        </span>
                      )}
                      <span className="flex-1">{log.message}</span>
                    </div>

                    {log.data && (
                      <div className="ml-2 mt-1">
                        <button
                          onClick={() => toggleJson(log.id)}
                          className="flex items-center gap-1 text-[10px] text-primary hover:underline"
                        >
                          {expandedJson.has(log.id) ? <ChevronUp className="h-3 w-3" /> : <ChevronDown className="h-3 w-3" />}
                          {expandedJson.has(log.id) ? 'Hide JSON' : 'Show JSON'}
                        </button>
                        {expandedJson.has(log.id) && (
                          <pre className="mt-1 overflow-x-auto rounded bg-gray-50 p-2 text-[10px] leading-relaxed">
                            {formatJson(log.data)}
                          </pre>
                        )}
                      </div>
                    )}

                    {log.screenshotUrl && (
                      <div className="ml-2 mt-1">
                        <button
                          onClick={() => setExpandedScreenshot(expandedScreenshot === log.id ? null : log.id)}
                          className="flex items-center gap-1 text-[10px] text-primary hover:underline"
                        >
                          {expandedScreenshot === log.id ? (
                            <><Minimize2 className="h-3 w-3" /> Hide Screenshot</>
                          ) : (
                            <><ImageIcon className="h-3 w-3" /> Show Screenshot</>
                          )}
                        </button>
                        {expandedScreenshot === log.id && (
                          <div className="mt-1 rounded border bg-gray-50 p-2">
                            <img
                              src={log.screenshotUrl}
                              alt="Page screenshot"
                              className="max-w-full rounded border"
                              style={{ maxHeight: 400 }}
                            />
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

function formatJson(json: string): string {
  try {
    return JSON.stringify(JSON.parse(json), null, 2);
  } catch {
    return json;
  }
}
