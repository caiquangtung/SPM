"use client";

import { useRef, useState } from "react";
import { useUploadFile, useAttachFileToTask } from "../hooks";
import { Upload, X, File as FileIcon } from "lucide-react";
import { toast } from "sonner";

interface FileUploadProps {
  taskId?: string;
  onFileUploaded?: (fileId: string) => void;
}

export default function FileUpload({
  taskId,
  onFileUploaded,
}: FileUploadProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const uploadFile = useUploadFile();
  const attachFile = useAttachFileToTask();

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      if (file.size > 100 * 1024 * 1024) {
        toast.error("File size must be less than 100MB");
        return;
      }
      setSelectedFile(file);
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) return;

    try {
      const uploadedFile = await uploadFile.mutateAsync(selectedFile);

      if (taskId) {
        await attachFile.mutateAsync({
          taskId,
          fileId: uploadedFile.id,
        });
      }

      setSelectedFile(null);
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
      onFileUploaded?.(uploadedFile.id);
    } catch (error) {
      // Error handled by hook
    }
  };

  const handleRemove = () => {
    setSelectedFile(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  return (
    <div className="space-y-2">
      <input
        ref={fileInputRef}
        type="file"
        onChange={handleFileSelect}
        className="hidden"
      />

      {!selectedFile ? (
        <button
          type="button"
          onClick={() => fileInputRef.current?.click()}
          className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
        >
          <Upload className="w-4 h-4" />
          Select File
        </button>
      ) : (
        <div className="flex items-center gap-2 p-3 bg-gray-50 rounded-lg border border-gray-200">
          <FileIcon className="w-5 h-5 text-gray-600" />
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-gray-900 truncate">
              {selectedFile.name}
            </p>
            <p className="text-xs text-gray-500">
              {(selectedFile.size / 1024 / 1024).toFixed(2)} MB
            </p>
          </div>
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={handleUpload}
              disabled={uploadFile.isPending || attachFile.isPending}
              className="px-3 py-1 text-sm font-medium text-white bg-blue-600 rounded hover:bg-blue-700 disabled:opacity-50"
            >
              {uploadFile.isPending || attachFile.isPending
                ? "Uploading..."
                : "Upload"}
            </button>
            <button
              type="button"
              onClick={handleRemove}
              className="p-1 text-gray-500 hover:text-red-600"
            >
              <X className="w-4 h-4" />
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

